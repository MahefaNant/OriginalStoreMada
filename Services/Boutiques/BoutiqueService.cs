using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;

namespace originalstoremada.Services.Boutiques;

public class BoutiqueService
{
    public static string BoutiquesAllName = "boutiquesAll";
    public static async Task<List<Boutique>> AllBoutique(ApplicationDbContext context)
    {
        var boutiques = context.Boutique.ToList();
        return boutiques;
    }

    public static async Task<Boutique> GetBoutique(ApplicationDbContext context ,int IdBoutique)
    {
        return await context.Boutique.FirstOrDefaultAsync(q => q.Id == IdBoutique);
    }

    public static void ChangeBoutiqueCookies(Boutique boutique, HttpContext httpContext)
    {
        if(httpContext.Request.Cookies[KeyStorage.KeyBoutiqueClient]!=null)
            httpContext.Response.Cookies.Delete(KeyStorage.KeyBoutiqueClient);
        httpContext.Response.Cookies.Append(KeyStorage.KeyBoutiqueClient, JsonConvert.SerializeObject(boutique), CookieFunction.OptionYear(1));
    }
}