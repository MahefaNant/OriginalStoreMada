using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Others;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.Others;
using originalstoremada.Models.Produits.views;
using originalstoremada.Models.Users;

namespace originalstoremada.Services.Produits;

public class PayementShopService
{

    private const double EarthRadiusKm = 6371.0;
    private const double maxRayon = 10;
    
    public static double PrixParKm = 1200;
    public static double PrixMinKm = 3000;

    static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = EarthRadiusKm * c;

        return distance;
    }

    static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }
    /*-----------------------------------------------------------------------------------------------*/

    static async Task<Boutique> RecupSurPlace(ApplicationDbContext context, string? IdBoutique)
    {
        Boutique? boutique;
        if (IdBoutique == null)
            throw new Exception("Veuillez choisir votre boutique");
        int IdB = int.Parse(IdBoutique.Split("/")[2]);
        boutique = await context.Boutique.FirstOrDefaultAsync(q => q.Id == IdB);
        return boutique;
    }

    static async Task<Dictionary<string, object>> PointLivraison(ApplicationDbContext context, Coordonner? coordonner)
    {

        Dictionary<string, object> res = new Dictionary<string, object>();
        Boutique boutique = null;

        if (coordonner.Ville == null || coordonner.Quartier == null || coordonner.Longitude == null ||
            coordonner.Latitude == null || coordonner.Ville == "" || coordonner.Quartier == "" ||
            coordonner.Longitude == 0 || coordonner.Latitude == 0)
            throw new Exception("Incomplete");
        double minDistance = double.MaxValue;
        var boutiques = await context.Boutique.ToListAsync();
        foreach (var b in boutiques)
        {
            double distance = CalculateDistance(coordonner.Latitude, coordonner.Longitude, b.Latitude, b.Longitude);
            if (distance < minDistance)
            {
                minDistance = distance;
                boutique = b;
            }
        }

        if (minDistance > maxRayon)
            throw new Exception($"Rayon maximal ({maxRayon} Km) depassé!");
        res["boutique"] = boutique;
        res["distance"] = minDistance;
        return res;
    }

    public static async Task<Boutique> ChooseBoutique(ApplicationDbContext context,
        Coordonner? coordonner, string? IdBoutique)
    {
        Boutique boutique = null;
        if (coordonner.Type != null)
        {
            if (coordonner.Type == 0)
            {
                boutique = await RecupSurPlace(context, IdBoutique);
            }
            else if (coordonner.Type == 1)
            {
                Dictionary<string, object> pointL = await PointLivraison(context, coordonner);
                boutique = (Boutique)pointL["boutique"];
                coordonner.Distance = (double)pointL["distance"];
            }
        }
        else throw new Exception("Veuillez choisir votre type de Livraison");

        return boutique;
    }

    public static InfoPayementShop InfoPayementShop(Coordonner coordonner, List<Cart> carts)
    {
        InfoPayementShop info = new InfoPayementShop();
        double sommeCart = ProduitShopService.SommeCart(carts);

        info.PrixParKm = PrixParKm;
        info.PrixMinKm = PrixMinKm;
        info.Type = coordonner.Type;
        info.Distance = coordonner.Distance;
        info.MontantPrix = sommeCart;

        return info;
    }

    public static async Task<List<Boutique>> BoutiqueOrderDistance(ApplicationDbContext context, Boutique initB)
    {
        var btqs = await context.Boutique.Where(q => q.Id != initB.Id).ToListAsync();
        foreach (var b in btqs)
        {
            b.DistParApport = CalculateDistance(initB.Latitude, initB.Longitude, b.Latitude, b.Longitude);
        }

        btqs = btqs.OrderBy(q => q.DistParApport).ToList();
        List<Boutique> res = new List<Boutique>() { initB };
        res.AddRange(btqs);
        return res;
    }

    public static async Task Payement(ApplicationDbContext context ,HttpContext httpContext, int id_typepayement)
    {
        var client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);
        Coordonner coordonner = JsonConvert.DeserializeObject<Coordonner>(httpContext.Session.GetString("coordonnerChooseBoutique"));
        coordonner.Boutique = await context.Boutique.FirstOrDefaultAsync(q => q.Id == coordonner.IdBoutique);
        var carts = await ProduitShopService.GetAllCart(context, httpContext);
        var info = InfoPayementShop(coordonner, carts);

        if (!carts.Any()) throw new Exception("Panier Vide");
        
        var facture = new Facture()
        {
            IdClient = client.Id, IdBoutique = coordonner.Boutique.Id,Date = DateTimeToUTC.Make(DateTime.Now)
        };
        context.Add(facture);
        await context.SaveChangesAsync();

        var stockPreferences = await context.VStockGlobalParPreference.Where(q => q.Stock!=null && q.Stock >0).ToListAsync();
            
        foreach (var c in carts)
        {
            var pref = stockPreferences.FirstOrDefault(q => q.IdPreferenceProduit == c.IdPref);
            if (pref == null || c.Quantiter > pref.Stock)
                throw new Exception("Stocks ou Quantiters Insuffisant! Modifier les quantiters");
            var payement = new PayementProduit()
            {
                IdFacture = facture.Id, IdTypePayement = id_typepayement, Date = facture.Date,
                IdPreferenceProduit = c.IdPref, Quantiter = c.Quantiter, Montant = c.PrixVente, Prix_achat = c.PrixAchat
            };

            context.Add(payement);
                
        }
            
        bool isInBoutique = coordonner.Type == 0 ? true : false;
        var adresseLivr = new AdresseLivraisonFacture()
        {
            IdFacture = facture.Id, Ville = coordonner.Boutique.Ville, Quartier = coordonner.Boutique.Quartier,
            Longitude = coordonner.Boutique.Longitude, Latitude = coordonner.Boutique.Latitude,
            IsInBoutque = isInBoutique, FraisLivraison = info.FraisLivraison()
        };
        context.Add(adresseLivr);
        await context.SaveChangesAsync();
        httpContext.Response.Cookies.Delete(KeyStorage.Carts);
    }

    public static async Task ValidSimulatePayement(ApplicationDbContext context, long id_facture)
    {
        DateTime nowUtc = DateTimeToUTC.Make(DateTime.Now);
        
        var facture = await context.Facture.FirstOrDefaultAsync(q => q.Id == id_facture);
        
        if (facture == null) throw new Exception("Facture introuvable!");
        
        facture.Date = DateTimeToUTC.Make(facture.Date);
        if(facture.DateLivrer!=null) facture.DateLivrer = DateTimeToUTC.Make((DateTime)facture.DateLivrer);
        facture.EstPayer = nowUtc;
        context.Update(facture);

        var payements = await context.PayementProduit
            .Include(q => q.PreferenceProduit)
            .Include(q => q.Facture)
            .ThenInclude(q => q.Boutique)
            .Where(q => q.IdFacture == id_facture)
            .ToListAsync();

        if (!payements.Any()) throw new Exception("Facture inutilisable!");

        Boutique boutique = payements[0].Facture.Boutique;

        List<Boutique> boutiqueOrderDistance = await BoutiqueOrderDistance(context, boutique);
        var stockPreferences = await context.VStockPreference.Where(q => q.Stock != null && q.Stock > 0).ToListAsync();

        foreach (var c in payements)
        {
            int QuantC = c.Quantiter;
            bool stopB = false;
            bool Insuf = true;
            
            foreach (var b in boutiqueOrderDistance)
            {
                Insuf = true;
                var pref = stockPreferences.FirstOrDefault(q =>
                    q.IdBoutique == b.Id && q.IdPreferenceProduit == c.IdPreferenceProduit);

                if (pref != null)
                {
                    if (QuantC <= pref.Stock)
                    {
                        stopB = true;
                        Insuf = false;
                        var sortie = new SortieProduit()
                        {
                            IdProduit = c.PreferenceProduit.IdProduit , IdPreferenceProduit = c.IdPreferenceProduit , Quantiter = QuantC , IdBoutique = b.Id,
                            Date = nowUtc
                        };
                        context.Add(sortie);
                    }
                    else
                    {
                        QuantC = (int)(QuantC - pref.Stock);
                        Insuf = false;
                        var sortie = new SortieProduit()
                        {
                            IdProduit = c.PreferenceProduit.IdProduit , IdPreferenceProduit = c.IdPreferenceProduit , Quantiter = (int)pref.Stock , IdBoutique = b.Id,
                            Date = nowUtc
                        };
                        context.Add(sortie);
                    }
                
                   
                }
                
                if(stopB) break;
            }
            
            if (Insuf) throw new Exception("Quantiter Insuffisant! Modifier les quantiters");
            
        }

    } 
    
    /*static async Task<List<VStockProduitBoutique>> OtherStockBoutique(ApplicationDbContext context, Boutique boutique,
        int id_produit)
    {
        var stocks = await context.VStockProduitBoutique
            .Where(q => q.IdBoutique != boutique.Id && q.Stock > 0 && q.IdProduit == id_produit)
            .OrderBy(q => CalculateDistance(boutique.Latitude, boutique.Longitude, q.Latitude, q.Longitude))
            .ToListAsync();
        return stocks;
    }

    public static async Task PossibilityLivraison(ApplicationDbContext context, HttpContext httpContext, int? id_boutique)
    {
        if (httpContext.Request.Cookies[KeyStorage.Carts] == null) throw new Exception("panier vide");
        var carts = JsonConvert.DeserializeObject<List<Cart>>(httpContext.Request.Cookies[KeyStorage.Carts]);
        var boutique = await context.Boutique.FindAsync(id_boutique);
        var allBoutique = await context.Boutique.ToListAsync();
        
        foreach (var b in allBoutique)
        {
            foreach (var c in carts)
            {
                
            }
        }
    }*/


    
}