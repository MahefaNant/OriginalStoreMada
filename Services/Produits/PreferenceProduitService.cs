using Microsoft.EntityFrameworkCore;
using originalstoremada.Data;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.Others;
using originalstoremada.Models.Produits.views;
using originalstoremada.Services.Repo;

namespace originalstoremada.Services.Produits;

public class PreferenceProduitService: ServiceRepo<PreferenceProduit>
{
    public override Pagination<PreferenceProduit> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }

    public static async Task<List<VStockProduit>> StockProduit(ApplicationDbContext context ,long? id_produit)
    {
        var res = await context.VStockProduit.Where(q => q.IdProduit == id_produit)
            .OrderBy(q => q.IdContenue)
            .ToListAsync();
        return res;
    }
    
    /*public static List<string> ColorsProduit(List<VStockProduit> vStockProduits)
    {
        List<string> res = vStockProduits
            .Select(q => q.Couleur)
            .Distinct()
            .Order()
            .ToList();
        return res;
    }*/

    /*public static List<PrefProd> PrefProds(List<VStockProduit> vStockProduits, List<string> colorsProduit)
    {
        List<PrefProd> res = new List<PrefProd>();

        foreach (var C in colorsProduit)
        {
            List<VStockProduit> VS = vStockProduits
                .Select(q => new VStockProduit
                    { Couleur = q.Couleur, Taille = q.Taille, IdPreferenceProduit = q.IdPreferenceProduit, Stock = q.Stock})
                .Where(q => q.Couleur == C)
                .ToList();
            Tailles[] tailles = new Tailles[VS.Count()];
            for(int i=0;i<tailles.Length;i++)
            {
                tailles[i] = new Tailles();
                tailles[i].Taille = VS[i].Taille;
                tailles[i].IdPref = VS[i].IdPreferenceProduit;
                tailles[i].Stock = VS[i].Stock;
            }
            res.Add(new PrefProd { Couleur = C , TaillesPref = tailles});
        }
        return res;
    }*/

    /*public static List<PrefProd> PrefProds(List<VStockProduit> vStockProduits)
    {
        var result = vStockProduits
            .GroupBy(q => q.IdContenue)
            .Select(group => new PrefProd
            {
                IdContenue = group.Key,
                TaillesPref = group.Select(item => new Tailles
                {
                    Taille = item.Taille,
                    IdPref = item.IdPreferenceProduit,
                    Stock = item.Stock
                }).ToArray()
            })
            .ToList();

        return result;
    }*/


}