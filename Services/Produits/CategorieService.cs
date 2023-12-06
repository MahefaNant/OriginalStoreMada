using Microsoft.EntityFrameworkCore;
using originalstoremada.Data;
using originalstoremada.Models.Produits;
using originalstoremada.Services.Repo;

namespace originalstoremada.Services.Produits;

public class CategorieService: ServiceRepo<CategorieService>
{
    public override Pagination<CategorieService> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }

    public static async Task<FraisImportation> FraisImportation(ApplicationDbContext context , int id_categorie)
    {
        var frais = await context.FraisImportation
            .OrderByDescending(q => q.DateDeb)
            .FirstOrDefaultAsync(q => q.IdCategorie == id_categorie);
        return frais;
    }

    public static async Task<Commission> Commission(ApplicationDbContext context, int id_categorie)
    {
        var frais = await context.Commission
            .OrderByDescending(q => q.DateDeb)
            .FirstOrDefaultAsync(q => q.IdCategorie == id_categorie);
        return frais;
    }

    public static async Task<List<CategorieProduit>> CategorieFillFraisAndCommission(ApplicationDbContext context, List<CategorieProduit> categorieProduits)
    {
        foreach (var q in categorieProduits)
        {
            q.FraisImportation = await FraisImportation(context, q.Id);
            q.Commission = await Commission(context, q.Id);
        }
        return categorieProduits;
    }
}