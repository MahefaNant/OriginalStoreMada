using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Devis;
using originalstoremada.Models.Evenements;
using originalstoremada.Models.Evenements.views;
using originalstoremada.Models.Users;

namespace originalstoremada.Services.Events;

public class EventService
{
    public static async Task AddToCart(ApplicationDbContext context , HttpContext httpContext , long IdEvent , long IdproduitEvent, int Quantiter)
    {
        DateTime now = DateTimeToUTC.Make(DateTime.Now);
        var events = await context.Evenement.FindAsync(IdEvent);
        if (!(DateTimeToUTC.Make(events.DateDeb) < now && DateTimeToUTC.Make(events.DateFin) > now))
        {
            // httpContext.Response.Cookies.Delete(KeyStorage.EventCarts);
            throw new Exception("Vous ne pouvez pas Intervenir");
        }
        if (Quantiter < 0) throw new Exception("Quantiter minimal est de un(1)");
        var P = await context.VProduitEventReste.Where(q => q.Id == IdproduitEvent).FirstOrDefaultAsync();
        if(P.QuantiterReste <= 0 ||  Quantiter > P.QuantiterReste ) throw new Exception("Stock Insuffisant");
        List<VProduitEventReste> carts = new List<VProduitEventReste>();
        var serializedCarts = httpContext.Request.Cookies[KeyStorage.EventCarts];
        if (!string.IsNullOrEmpty(serializedCarts))
        {
            try
            {
                carts = JsonConvert.DeserializeObject<List<VProduitEventReste>>(serializedCarts);
                carts.RemoveAll(q => q.IdEvenement != IdEvent);
            }
            catch (Exception e)
            {
                throw new Exception("Une erreur est survenue");
            }
                
        }
        
        bool isExist = false;
        
        for (int i = 0; i < carts.Count; i++)
        {
            if (carts[i].Id == IdproduitEvent)
            {
                carts[i].QuantiterReste += Quantiter;
                if(P.QuantiterReste <= 0 ||  carts[i].QuantiterReste > P.QuantiterReste ) throw new Exception("Stock Insuffisant");
                isExist = true;
                break;
            }
        }

        if (!isExist)
        {
            P.QuantiterReste = Quantiter;
            carts.Add(P);
        }
        
        httpContext.Response.Cookies.Delete(KeyStorage.EventCarts);
        httpContext.Response.Cookies.Append(KeyStorage.EventCarts, JsonConvert.SerializeObject(carts), CookieFunction.OptionHour(1));
    }
    
    public static async Task UpdateCart(ApplicationDbContext context,HttpContext httpContext,long IdEvent, long IdproduitEvent, int Quantiter)
    {
        DateTime now = DateTimeToUTC.Make(DateTime.Now);
        var events = await context.Evenement.FindAsync(IdEvent);
        if (!(DateTimeToUTC.Make(events.DateDeb) < now && DateTimeToUTC.Make(events.DateFin) > now))
        {
            httpContext.Response.Cookies.Delete(KeyStorage.EventCarts);
            throw new Exception("Vous ne pouvez pas Intervenir");
        }
        
        List<VProduitEventReste> carts = JsonConvert.DeserializeObject<List<VProduitEventReste>>(httpContext.Request.Cookies[KeyStorage.EventCarts]);
        long Id = 0;
        foreach (var C in carts)
        {
            if (C.Id == IdproduitEvent)
            {
                C.QuantiterReste = Quantiter;
                Id = C.Id;
                break;
            }
        }
        var P = await context.VProduitEventReste.FindAsync(IdproduitEvent);
        if(P.QuantiterReste <= 0 &&  Quantiter > P.QuantiterReste ) throw new Exception("Stock Insuffisant");
        
        httpContext.Response.Cookies.Delete(KeyStorage.EventCarts);
        httpContext.Response.Cookies.Append(KeyStorage.EventCarts, JsonConvert.SerializeObject(carts), CookieFunction.OptionDay(7));
    }

    public static void DeleteCarts(HttpContext httpContext, long IdproduitEvent)
    {
        List<VProduitEventReste> carts = JsonConvert.DeserializeObject<List<VProduitEventReste>>(httpContext.Request.Cookies[KeyStorage.EventCarts]);
        carts.RemoveAll(q => q.Id == IdproduitEvent);
        httpContext.Response.Cookies.Delete(KeyStorage.EventCarts);
        if(carts.Any())
            httpContext.Response.Cookies.Append(KeyStorage.EventCarts, JsonConvert.SerializeObject(carts), CookieFunction.OptionHour(1));
    }
    
    public static double[] SommeCart(List<VProduitEventReste> carts)
    {
        double[] res = {0,0};
        foreach (var q in carts)
        {
            res[0] +=  Math.Round(q.Prix * q.QuantiterReste,2);
            res[1] += Math.Round((double)q.PrixAriary * q.QuantiterReste ,2);
        }
        return res;
    }

    public static async Task<long> ToDevis(ApplicationDbContext context, HttpContext httpContext, long IdEvent)
    {
        Client client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);
        List<VProduitEventReste> prods =
            JsonConvert.DeserializeObject<List<VProduitEventReste>>(httpContext.Request.Cookies[KeyStorage.EventCarts]);
        List<VProduitEventReste> ver = prods.Where(q => q.IdEvenement == IdEvent).ToList();
        foreach (var P in prods)
        {
            if (ver.Any(v => v.Id == P.Id && v.QuantiterReste < P.QuantiterReste))
            {
                throw new Exception("Quantité insuffisante");
            }
        }

        DateTime now = DateTimeToUTC.Make(DateTime.Now);
        
        var devis = new Models.Devis.Devis()
        {
            IdClient = client.Id , Date = now, DateEnvoi = now, DateValidation = now 
        };
        context.Add(devis);
        await context.SaveChangesAsync();
        foreach (var P in prods)
        {
            var interaction = new InteractionEvent()
            {
                IdEvenement = IdEvent, IdClient = client.Id , Quantiter = P.QuantiterReste , IdProduitEvent = P.Id
            };
            
            var commandeDevis = new CommandeDevis()
            {
                IdDevis = devis.Id , IdCategorie = P.IdCategorie , ProduitName = P.Nom , PrixEuro = P.Prix , Couleur = P.Couleur , Taille = P.Taille ,
                Nombre = P.QuantiterReste, ReferenceSite = $"depuis l`évènement #{IdEvent}", image = P.Image
            };
            context.Add(interaction);
            context.Add(commandeDevis);
        }

        await context.SaveChangesAsync();
        httpContext.Response.Cookies.Delete(KeyStorage.EventCarts);
        return devis.Id;
    }
    
}