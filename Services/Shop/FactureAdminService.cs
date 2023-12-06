using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutique;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.Users;
using originalstoremada.Services.Mail;
using originalstoremada.Services.Users;

namespace originalstoremada.Services.Shop;

public class FactureAdminService
{
    public static async Task<Dictionary<string, object>> Factures(ApplicationDbContext context, HttpContext httpContext,int sizeList, int? pagId, int etat = 0)
    {
        Dictionary<string, object> res = new Dictionary<string, object>();
        Admin A = JsonConvert.DeserializeObject<Admin>(httpContext.Request.Cookies[KeyStorage.KeyAdmin]);
        Admin admin = await context.Admin.FindAsync(A.Id);
        IQueryable<VFacture> facts = context.VFacture
            .Include(q => q.Client)
            .Include(q => q.Boutique);
        if (!AdminService.IsLevel_5(admin))
        {
            AffectationEmployer? affectation = await context.AffectationEmployer
                .Where(q => q.DateFin == null && q.IdAdmin == admin.Id)
                .OrderByDescending(q => q.DateDeb)
                .FirstOrDefaultAsync();
            facts = facts.Where(q => q.IdBoutique == affectation.IdBoutique);
        }
        /*-------------RECHERCHE------------------*/
        facts = MyShopService.FiltreFacture(facts, etat);
        /*----------------------------------------*/

        facts = facts.OrderByDescending(q => q.Date);
        Pagination<VFacture> pagination = new Pagination<VFacture>(sizeList, pagId, facts);
        facts = pagination.Paginate();
        List<VFacture> F = await facts.ToListAsync();
        MyShopService.SetEtatListFacture(F);
        res.Add("pagination", pagination);
        res.Add("myFacture", F);
        return res;
    }

    public static async Task SetProduitPret(ApplicationDbContext context , SmtpConfig smtpConfig , DateTime? dateEstim ,long id_facture = 0)
    {
        var adresse = await context.AdresseLivraisonFacture
            .FirstOrDefaultAsync(q => q.IdFacture == id_facture);
        if (adresse != null)
        {
            adresse.DatePret = DateTimeToUTC.Make(DateTime.Now);
            if (!adresse.IsInBoutque && dateEstim != null)
            {
                if (DateTimeToUTC.Make((DateTime)dateEstim) < adresse.DatePret) throw new Exception("Date non cohérant");
                adresse.DateEstimation = DateTimeToUTC.Make((DateTime)dateEstim);
            }
            context.Update(adresse);
            await context.SaveChangesAsync();

            VFacture facture = await context.VFacture.FirstOrDefaultAsync(q => q.Id == id_facture);

            Client client = await context.Client.FindAsync(facture.IdClient);

            MailBody.ProduitFactureAfterPret(smtpConfig, client.Mail, id_facture, facture);

        }
    }

    public static async Task FinaliserLivraison(ApplicationDbContext context, SmtpConfig smtpConfig , long id_facture = 0)
    {
        var facture = await context.Facture
            .FirstOrDefaultAsync( q => q.Id == id_facture);
        if (facture != null)
        {
            facture.Date = DateTimeToUTC.Make(facture.Date);
            facture.EstPayer = DateTimeToUTC.Make((DateTime)facture.EstPayer);
            facture.DateLivrer = DateTimeToUTC.Make(DateTime.Now);
            context.Update(facture);
            await context.SaveChangesAsync();
            Client client = await context.Client.FirstOrDefaultAsync(q => q.Id == facture.IdClient);
            MailBody.MessageFinalisationLivraison(smtpConfig, facture, client);
        }
    }
    
}