using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Others;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.Others;
using originalstoremada.Models.Produits.views;
using originalstoremada.Models.Users;
using originalstoremada.Services.Repo;
using Image = System.Drawing.Image;

namespace originalstoremada.Services.Produits;

public class ProduitShopService : ServiceRepo<VStockProduitGlobal>
{

    public override Pagination<VStockProduitGlobal> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }
    
    public ProduitShopService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VStockProduitGlobal>> Produits(HttpContext httpContext,int sizeList, int? pagId , string? NomProd , int? IdCategorie , int[]? Type, string? Prix, bool PourEnfant, bool EnPromotion)
    {
        IQueryable<VStockProduitGlobal> prods = _context.VStockProduitGlobal
                .OrderByDescending(q => q.Id)
            .Where(q => q.PrixVenteInitial!=null);
        // FindNomProd(prods, NomProd);
        prods = FindCategorie(httpContext,prods, IdCategorie);
        prods = FindType(httpContext,prods, Type);
        prods = FindPrix(httpContext,prods, Prix);
        prods = FindEnfant(httpContext,prods, PourEnfant);
        prods = FindProm(httpContext,prods, EnPromotion);
        Pagination = new Pagination<VStockProduitGlobal>(sizeList, pagId, prods);
        prods = Pagination.Paginate();
        return await prods.ToListAsync();
    }

    public static async Task<VStockProduitGlobal> ProduitDetails(ApplicationDbContext db ,long? id_produit)
    {
        var prod = await db.VStockProduitGlobal
            .Include(q => q.CategorieProduit)
            .FirstOrDefaultAsync(q => q.Id == id_produit);
        return prod;
    }

    public static async Task<List<Cart>> GetAllCart(ApplicationDbContext context, HttpContext httpContext)
    {
        List<Cart> carts = new List<Cart>();
        carts = JsonConvert.DeserializeObject<List<Cart>>(httpContext.Request.Cookies[KeyStorage.Carts]);
        if (!carts.Any()) throw new Exception("Une Erreur s` est Produite");
        long[] idPrefs = carts.Select(q => q.IdPref).ToArray();
        idPrefs = idPrefs.OrderBy(id => id).ToArray();

        var prefs = await context.VStockGlobalParPreference.OrderBy(q => q.IdPreferenceProduit).Where(q => idPrefs.Contains(q.IdPreferenceProduit)).ToListAsync();

        var query = from c in carts
            join p in context.VCartProduit.Include(vp => vp.ContenueProduit)
                on c.IdPref equals p.IdPreferenceProduit
            select new
            {
                c,
                p.categorie,
                Couleur = p.ContenueProduit != null ? p.ContenueProduit.Couleur : null,
                p.Taille,
                p.Nom,
                Image = p.ContenueProduit != null ? p.ContenueProduit.Image : null,
                p.PrixVenteProm,
                p.PrixAchat
            };

        var results = query.ToList();
        

        for (int i = 0; i < results.Count; i++)
        {
            if(carts[i].Quantiter > prefs[i].Stock) throw new Exception($"Stock insuffisant ( Reste: { prefs[i].Stock} )");
                
            carts[i].Categorie = results[i].categorie;
            carts[i].Couleur = results[i].Couleur;
            carts[i].Taille = results[i].Taille;
            carts[i].Nom = results[i].Nom;
            carts[i].Image = results[i].Image;
            carts[i].PrixVente = (double)results[i].PrixVenteProm;
            carts[i].PrixAchat = (double)results[i].PrixAchat;
        }

        return carts;
    }

    public static async Task Cart(ApplicationDbContext context, HttpContext httpContext, long id_produit, long id_pref, int quantiter)
    {
        List<Cart> carts = new List<Cart>();

         // Désérialisez les cookies une seule fois s'ils existent.
            var serializedCarts = httpContext.Request.Cookies[KeyStorage.Carts];
            if (!string.IsNullOrEmpty(serializedCarts))
            {
                try
                {
                    carts = JsonConvert.DeserializeObject<List<Cart>>(serializedCarts);
                }
                catch (Exception e)
                {
                    throw new Exception("Une erreur est survenue");
                }
                
            }

            bool isExist = false;

            var stockPref = await context.VStockGlobalParPreference
                .FirstOrDefaultAsync(q => q.IdProduit == id_produit && q.IdPreferenceProduit == id_pref);

            // Utilisez une boucle for au lieu de foreach pour améliorer les performances.
            for (int i = 0; i < carts.Count; i++)
            {
                if (carts[i].IdProduit == id_produit && carts[i].IdPref == id_pref)
                {
                    carts[i].Quantiter += (int)quantiter;
                    isExist = true;
                    if (stockPref == null || carts[i].Quantiter > stockPref.Stock) throw new Exception($"Stock insuffisant ( Reste: {stockPref.Stock} )");
                    break;
                }
            }

            if (!isExist)
            {

                Console.WriteLine(JsonConvert.SerializeObject(stockPref));
                Console.WriteLine(quantiter);
                if (stockPref == null || quantiter > stockPref.Stock) throw new Exception($"Stock insuffisant ( Reste: {stockPref.Stock} )");
                
                var stockQuery = from q in context.VImagePrincipalPrixProduit.Where(q => q.Id == id_produit)
                     join c in context.CategorieProduit on q.IdCategorie equals c.Id
                     select new
                     {
                         q.Id, q.IdCategorie, q.Image, q.PrixVenteProm,
                         NomProduit = q.Nom,
                         Categorie = c.Nom
                     };

                var produitsQuery = from s in context.VStockProduit
                        .Include(q => q.ContenueProduit)
                    .Where(q => q.IdProduit == id_produit && q.IdPreferenceProduit == id_pref)
                    join p in stockQuery on s.IdProduit equals p.Id
                    select new
                    {
                        s.IdProduit, s.IdPreferenceProduit, s.Stock, s.Taille, s.ContenueProduit.Couleur,
                        p.IdCategorie, p.NomProduit, p.Categorie, Image = s.ContenueProduit!=null?  s.ContenueProduit.Image : null, p.PrixVenteProm
                    };

                var res = await produitsQuery.FirstAsync();

                var CART = new Cart()
                {
                    Couleur = res.Couleur,
                    Taille = res.Taille,
                    Nom = res.NomProduit,
                    Image = res.Image,
                    Categorie = res.Categorie,
                    IdProduit = res.IdProduit,
                    PrixVente = (double)res.PrixVenteProm,
                    IdPref = res.IdPreferenceProduit,
                    Quantiter = quantiter
                };

                carts.Add(CART);
            }

            // Sérialisez et mettez à jour les cookies une seule fois.
            httpContext.Response.Cookies.Delete(KeyStorage.Carts);
            httpContext.Response.Cookies.Append(KeyStorage.Carts, JsonConvert.SerializeObject(carts), CookieFunction.OptionDay(7));
    }

    public static double SommeCart(List<Cart>? carts)
    {
        double res = 0;
        if(carts!=null)
            foreach (var q in carts)
            {
                res += q.PrixVente * q.Quantiter;
            }

        return res;
    }

    public static async Task UpdateCarts(ApplicationDbContext context,HttpContext httpContext, long[] idPrefs, int[] quants)
    {
        List<Cart> carts = JsonConvert.DeserializeObject<List<Cart>>(httpContext.Request.Cookies[KeyStorage.Carts]);
        Dictionary<long, Cart> cartDict = carts.ToDictionary(cart => cart.IdPref);

        idPrefs = idPrefs.OrderBy(id => id).ToArray();
        var prefs = await context.VStockGlobalParPreference.OrderBy(q => q.IdPreferenceProduit).Where(q => idPrefs.Contains(q.IdPreferenceProduit)).ToListAsync();

        for (int i=0;i< idPrefs.Length; i++)
        {
            int quantiter = quants[i] > 0 ? quants[i] : 1;
            if(prefs[i].Stock < quantiter) throw new Exception($"Stock insuffisant ( Reste: {prefs[i].Stock} )");
            long idPref = idPrefs[i];

            if (cartDict.TryGetValue(idPref, out Cart cartToUpdate))
            {
                cartToUpdate.Quantiter = quantiter;
            }
        }
        carts = cartDict.Values.ToList();
        httpContext.Response.Cookies.Delete(KeyStorage.Carts);
        httpContext.Response.Cookies.Append(KeyStorage.Carts, JsonConvert.SerializeObject(carts), CookieFunction.OptionDay(7));
    }

    /*--------------------------------------RECHERCHE-------------------------------------------------*/

    public static List<Price> PricesCherch()
    {
        List<Price> prices = new List<Price>();
        prices.Add(new Price(0,15000));
        prices.Add(new Price(15000,30000));
        prices.Add(new Price(30000,45000));
        prices.Add(new Price(45000,60000));
        return prices;
    }

    IQueryable<VStockProduitGlobal> FindNomProd(HttpContext httpContext ,IQueryable<VStockProduitGlobal> prods, string? NomProd)
    {
        if (!NomProd.IsNullOrEmpty())
        {
            prods = prods.Where(q => EF.Functions.ILike(q.Nom, $"%{NomProd}%"));
        }

        return prods;
    }
    
    IQueryable<VStockProduitGlobal> FindCategorie(HttpContext httpContext ,IQueryable<VStockProduitGlobal> prods, int? IdCategorie)
    {
        if (IdCategorie != null)
        {
            prods = prods.Where(q => q.IdCategorie == IdCategorie);
            httpContext.Session.SetInt32("IdCategorie" , (int)IdCategorie);
        }
        else
        {
            if(httpContext.Session.GetInt32(Recherche.IdCategorieName)!=null) httpContext.Session.Remove(Recherche.IdCategorieName);
        }

        return prods;
    }
    
    IQueryable<VStockProduitGlobal> FindType(HttpContext httpContext ,IQueryable<VStockProduitGlobal> prods, int[]? Type)
    {
        if (Type != null && Type.Length > 0)
        {
            prods = prods.Where(q => Type.Contains(q.IdType));
            httpContext.Session.SetString(Recherche.TypeName ,JsonConvert.SerializeObject(Type));
        }
        else
        {
            if(httpContext.Session.GetString(Recherche.TypeName)!=null) httpContext.Session.Remove(Recherche.TypeName);
        }

        return prods;
    }
    
    IQueryable<VStockProduitGlobal> FindPrix(HttpContext httpContext ,IQueryable<VStockProduitGlobal> prods, string? Prix)
    {
        if (Prix != null)
        {
            double M = PricesCherch().Last().Max;
            string[] P = Prix.Split("-");
            double min = Math.Round(Convert.ToDouble(P[0]) , 2);
            double? max;
            if (P.Length == 1 && min >= M)
            {
                prods = prods.Where(q => q.PrixVenteProm >= min);
                httpContext.Session.SetString(Recherche.Price , $"{min}");
            } else if (P.Length > 1)
            {
                max =  Math.Round(Convert.ToDouble(P[1]) , 2);
                prods = prods.Where(q => q.PrixVenteProm > min && q.PrixVenteProm <=max);
                httpContext.Session.SetString(Recherche.Price , $"{min}-{max}");
            }
            else
            {
                if(httpContext.Session.GetString(Recherche.Price)!=null) httpContext.Session.Remove(Recherche.Price);
            }
        }
        else
        {
            if(httpContext.Session.GetString(Recherche.Price)!=null) httpContext.Session.Remove(Recherche.Price);
        }
        return prods;
    }
    
    IQueryable<VStockProduitGlobal> FindEnfant(HttpContext httpContext ,IQueryable<VStockProduitGlobal> prods, bool PourEnfant)
    {
        if (PourEnfant)
        {
            prods = prods.Where(q => q.PourEnfant == true);
            httpContext.Session.SetString(Recherche.PourEnfant , "true");
        }
        else
        {
            if(httpContext.Session.GetString(Recherche.PourEnfant)!=null) httpContext.Session.Remove(Recherche.PourEnfant);
        }
        return prods;
    }
    
    IQueryable<VStockProduitGlobal> FindProm(HttpContext httpContext ,IQueryable<VStockProduitGlobal> prods, bool EnPromotion)
    {
        if (EnPromotion)
        {
            prods = prods.Where(q => q.DateDebPromo!=null);
            httpContext.Session.SetString(Recherche.EnPromotion , "true");
        }
        else
        {
            if(httpContext.Session.GetString(Recherche.EnPromotion)!=null) httpContext.Session.Remove(Recherche.EnPromotion);
        }

        return prods;
    }
    /*-----------------------------------------------------------------------------------------------*/
    /*-------------------------------------CHECKED----------------------------------------------------------*/
    
    public static List<TypeProduit> CheckedTypeProduit(HttpContext httpContext ,List<TypeProduit> typeProduits)
    {
        int?[] res;
        if (httpContext.Session.GetString(Recherche.TypeName) != null)
        {
            try
            {
                int[] Type = JsonConvert.DeserializeObject<int[]>(httpContext.Session.GetString(Recherche.TypeName));
                foreach (var t in Type)
                {
                    foreach (var q in typeProduits)
                    {
                        if (q.Id == t)
                        {
                            q.IsCheked = true;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
        return typeProduits;
    }
    
    /*-----------------------------------------------------------*/

    public static async Task<List<VStockProduitGlobal>> FillIsMyFavoris(ApplicationDbContext context , HttpContext httpContext , List<VStockProduitGlobal> produits)
    {
        if (httpContext.Request.Cookies[KeyStorage.KeyClient] == null)
        {
            return produits;
        }

        try
        {
            Client client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);

            var favoris = await context.FavorisProduit
                .Where(q => q.IdClient == client.Id)
                .Select(f => f.IdProduit)
                .ToListAsync();

            if (favoris.Any())
            {
                var favorisSet = new HashSet<long>(favoris.Select(f => f));
                
                foreach (var p in produits)
                {
                    if (favorisSet.Contains(p.Id))
                    {
                        p.IsMyFavorite = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Gérez l'exception de manière appropriée
        }

        return produits;
    }
    
    /*---------------------------------------*/

    public static async Task<int> SetFavoris(ApplicationDbContext context, HttpContext httpContext, int IdProduit)
    {
        // 0 : not logged
        // 1 : add
        // 2 : remove
        // 3 : Error
        if (httpContext.Request.Cookies[KeyStorage.KeyClient] == null) return 0;

        Client client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);

        var fav = await context.FavorisProduit
            .FirstOrDefaultAsync(q => q.IdProduit == IdProduit && q.IdClient == client.Id);
        if (fav != null)
        {
            context.Remove(fav);
            await context.SaveChangesAsync();
            return 2;
        }

        fav = new FavorisProduit()
        {
            IdProduit = IdProduit, IdClient = client.Id,Date = DateTimeToUTC.Make(DateTime.Now)
        };
        context.Add(fav);
        await context.SaveChangesAsync();
        return 1;
    }

}