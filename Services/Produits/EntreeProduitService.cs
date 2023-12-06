using Microsoft.EntityFrameworkCore;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Users;
using originalstoremada.Services.Repo;
using originalstoremada.Services.Users;

namespace originalstoremada.Services.Produits;

public class EntreeProduitService: ServiceRepo<EntreeProduit>
{
    public override Pagination<EntreeProduit> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }

    public static async Task AjoutProduitDynamique(ApplicationDbContext context, EntreeProduit entreeProduit, Admin admin,Boutique? boutique,
        long? IdContenue, string taille)
    {
        var preference = await CheckPreference(context, entreeProduit, IdContenue, taille);
        entreeProduit.IdPreferenceProduit = preference.Id;
        if (!AdminService.IsLevel_5(admin)) entreeProduit.IdBoutique = boutique.Id;
        context.Add(entreeProduit);
    }

    private static async  Task<PreferenceProduit> CheckPreference(ApplicationDbContext context,EntreeProduit entreeProduit,long? IdContenue, string taille)
    {
        var preference = new PreferenceProduit
        {
            IdProduit = entreeProduit.IdProduit,
            Taille = taille, 
            IdContenue = (long)IdContenue
        };
        var existingPreference = await context.PreferenceProduit
            .FirstOrDefaultAsync(p => p.IdProduit == preference.IdProduit && p.Taille == preference.Taille && p.IdContenue == preference.IdContenue);

        if (existingPreference != null)
        {
            return existingPreference;
        }

        context.PreferenceProduit.Add(preference);
        await context.SaveChangesAsync();
        return preference;
    }
    
    private static async Task<bool> CheckConstraintViolationAsync(ApplicationDbContext context, PreferenceProduit preference)
    {
        return await context.PreferenceProduit
            .AnyAsync(p => p.IdProduit == preference.IdProduit && p.Taille == preference.Taille && p.IdContenue == preference.IdContenue);
    }
}