using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.Users;

namespace originalstoremada.Services.Shop;

public class MyShopService
{
    public static async Task<Dictionary<string, object>> MyFacture(ApplicationDbContext context, HttpContext httpContext,int sizeList, int? pagId, int etat = 0)
    {
        Dictionary<string, object> res = new Dictionary<string, object>();
        Client client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);
        IQueryable<VFacture> facts = context.VFacture
            .Include(q => q.Client)
            .Include(q => q.Boutique)
            .Where(q => q.IdClient == client.Id);
        
        /*-------------RECHERCHE------------------*/
        facts = FiltreFacture(facts, etat);
        /*----------------------------------------*/

        facts = facts.OrderByDescending(q => q.Date);
        Pagination<VFacture> pagination = new Pagination<VFacture>(sizeList, pagId, facts);
        facts = pagination.Paginate();
        List<VFacture> F = await facts.ToListAsync();
        SetEtatListFacture(F);
        res.Add("pagination", pagination);
        res.Add("myFacture", F);
        return res;
    }

    public static IQueryable<VFacture> FiltreFacture(IQueryable<VFacture> facts, int etat)
    {
        if (etat == 0) ;
        else if (etat == 1)
        {
            facts = facts.Where( q => q.IsInBoutique && q.DatePret == null && q.DateLivrer == null);
        } else if (etat == 2)
        {
            facts = facts.Where(q => q.IsInBoutique && q.DatePret != null && q.DateLivrer == null);
        } else if (etat == 3)
        {
            facts = facts.Where(q => q.IsInBoutique && q.DatePret != null && q.DateLivrer!=null);
        }
        
        else if (etat == 4)
        {
            facts = facts.Where(q => !q.IsInBoutique && q.DatePret == null && q.DateLivrer == null);
        } else if (etat == 5)
        {
            facts = facts.Where(q => !q.IsInBoutique && q.DatePret != null && q.DateLivrer == null);
        } else if (etat == 6)
        {
            facts = facts.Where(q => !q.IsInBoutique && q.DatePret != null && q.DateLivrer!=null);
        }

        return facts;
    }
    
    public static void SetEtatListFacture(List<VFacture> factures)
    {
        foreach (var q in factures)
        {
            SetEtatFacture(q);
        }
    }

    public static void SetEtatFacture(VFacture q)
    {
        if (q.IsInBoutique && q.DatePret == null && q.DateLivrer == null)
        {
            q.EtatLivraisonType = "Récupération directe";
            q.EtatLivraison = "Pas encore prêt(En attente)";
        } 
        //2 
        else if (q.IsInBoutique && q.DatePret != null && q.DateLivrer == null)
        {
            q.EtatLivraisonType = "Récupération directe";
            q.EtatLivraison = "Pret à être récupéré";
        } 
        //3
        else if (q.IsInBoutique && q.DatePret != null && q.DateLivrer!=null)
        {
            q.EtatLivraisonType = "Récupération directe";
            q.EtatLivraison = "Déjà récupérer";
        }
            
        // 4livraison
        if (!q.IsInBoutique && q.DatePret == null && q.DateLivrer == null)
        {
            q.EtatLivraisonType = "Par livraison";
            q.EtatLivraison = "Pas encore prêt(En attente)";
        } 
        //5
        else if (!q.IsInBoutique && q.DatePret != null && q.DateLivrer == null)
        {
            q.EtatLivraisonType = "Par livraison";
            q.EtatLivraison = "Pret a livré";
        } 
        //6
        else if (!q.IsInBoutique && q.DatePret != null && q.DateLivrer!=null)
        {
            q.EtatLivraisonType = "Par livraison";
            q.EtatLivraison = "Déjà Livrer";
        }
    }

    public static async Task<List<VPayementProduit>> FactureDetails(ApplicationDbContext context  ,long id_facture = 0)
    {
        var res = await context.VPayementProduit
            .Include(q => q.PreferenceProduit)
            .ThenInclude(q => q.ContenueProduit)
            .ThenInclude(q => q.Produit)
            .ThenInclude(q => q.CategorieProduit)
            .Where(q => q.IdFacture == id_facture)
            .ToListAsync();

        return res;
    }

    public static Dictionary<int, string> SuivisFactureEtat(VFacture facture)
    {
        Dictionary<int, string> res = new Dictionary<int, string>();
        for(int i=1;i<= 3 ;i++) res.Add(i, "");
        if (facture.EstPayer == null)
        {
            res[1] = "active";
        } else if (facture.DatePret == null)
        {
            res[1] = "completed";
            res[2] = "active";
        } else if (facture.DatePret != null)
        {
            if (facture.DateLivrer == null)
            {
                res[1] = "completed";
                res[2] = "completed";
                res[3] = "active";
            } else if (facture.DateLivrer != null)
            {
                res[1] = "completed";
                res[2] = "completed";
                res[3] = "completed active";
            }
        }

        return res;
    }

}