using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutique;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.StatAdmin;
using originalstoremada.Models.Users;
using originalstoremada.Services;
using originalstoremada.Services.Mail;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Shop;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.FactureAdmin;

public class FactureAdminController : Controller
{
    
    private readonly ApplicationDbContext _context;
    private readonly SmtpConfig _smtpConfig;

    public FactureAdminController(ApplicationDbContext context, SmtpConfig smtpConfig)
    {
        _context = context;
        _smtpConfig = smtpConfig;
    }

    // GET
    [CheckAdminAll] 
    public async Task<IActionResult> Facture(int? pagId, int etat)
    {
        var facts = new List<VFacture>();
        try
        {
            Dictionary<string, object> MyFACTS = await FactureAdminService.Factures(_context, HttpContext, 10, pagId, etat);
            ViewBag.pagination = MyFACTS["pagination"];
            facts = (List<VFacture>)MyFACTS["myFacture"];
            ViewBag.etat = etat;
        }
        catch (Exception e)
        {
            // ignored
        }
        return View(facts);
    }
    
    [CheckAdminAll]
    public async Task<IActionResult> DetailsFacture(long id_facture = 0)
    {
        try
        {
            List<VPayementProduit> payementProduits = await MyShopService.FactureDetails(_context, id_facture);
            var vFacture = await _context.VFacture
                .Include(q => q.Client)
                .Include(q => q.Boutique)
                .FirstOrDefaultAsync(q => q.Id == id_facture);

        
            MyShopService.SetEtatFacture(vFacture);
            ViewBag.facture = vFacture;
            ViewBag.suivisFacture = MyShopService.SuivisFactureEtat(vFacture);
        

            ViewBag.client = await _context.Client.FirstOrDefaultAsync(q => q.Id == vFacture.IdClient);
        
            if (vFacture.EstPayer == null) ViewBag.typePayementForm = await _context.TypePayement.FirstOrDefaultAsync(q => q.Id == vFacture.IdTypePayement);
        
            ViewBag.adresseLivraison = await _context.AdresseLivraisonFacture.FirstOrDefaultAsync(q => q.IdFacture == id_facture);

            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
        
            return View(payementProduits);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Facture));
        }
        
    }

    [CheckAdminAll]
    public async Task<IActionResult> SetProduitPret(DateTime? dateEstim, long id_facture = 0)
    {
        try
        {
            await FactureAdminService.SetProduitPret(_context, _smtpConfig , dateEstim, id_facture);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }

        return RedirectToAction(nameof(DetailsFacture), new { id_facture });
    }

    [CheckAdminAll]
    public async Task<IActionResult> ValiderPayement(long id_facture)
    {
        try
        {
            await PayementShopService.ValidSimulatePayement(_context, id_facture);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }

        return RedirectToAction(nameof(DetailsFacture) , new { id_facture });
    }
    
    [CheckAdminAll]
    public async Task<IActionResult> SuspendrePayement(long id_facture)
    {
        try
        {
            var facture = await _context.Facture.FindAsync(id_facture);
            facture.Date = DateTimeToUTC.Make(facture.Date);
            if(facture.DateLivrer!=null) facture.DateLivrer = DateTimeToUTC.Make((DateTime)facture.DateLivrer);
            if(facture.EstPayer!=null) facture.EstPayer = DateTimeToUTC.Make((DateTime)facture.EstPayer);
            facture.DateSuspendue = DateTimeToUTC.Make(DateTime.Now);
            _context.Update(facture);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = "Facture inaccessible";
        }

        return RedirectToAction(nameof(DetailsFacture) , new { id_facture });
    }

    [CheckAdminAll]
    public async Task<IActionResult> FinaliserLivraison(long id_facture = 0)
    {
        try
        {
            await FactureAdminService.FinaliserLivraison(_context, _smtpConfig, id_facture);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(DetailsFacture), new { id_facture });
    }
    
    [CheckAdminLevel5]
    public async Task<IActionResult> StatIndex(int? Annee, int? pagId, bool isPagination = false)
    {
        try
        {
            if (Annee == null)
            {
                DateTime D = DateTimeToUTC.Make(DateTime.Now);
                Annee = D.Year;
            }
            
            
            if (!isPagination) pagId = 1;

            IQueryable<PayementProduit> queryPayd = _context.PayementProduit
                .Include(q => q.TypePayement)
                .Include(q => q.Facture)
                .ThenInclude(q => q.Client)
                .OrderByDescending(q => q.Date)
                .Where(q => q.Date.Year == Annee && q.Facture.EstPayer!=null);

            Pagination<PayementProduit> paginations = new Pagination<PayementProduit>(10, pagId, queryPayd);
            paginations.Paginate();

            var model = await queryPayd.ToListAsync();
            
            List<double> payDatas = new List<double>();
            List<string> labelMois = new List<string>();

            List<VBeneficeReelFactureParAnsMois> benefs = await _context.VBeneficeReelFactureParAnsMois
                .Where(q => q.Annee == Annee).ToListAsync();
            
            foreach (var d in benefs)
            {
                labelMois.Add(UtilesFonctions.Mois()[d.Mois - 1]);
                payDatas.Add(d.TotalBenefice);
            }
            
            ViewBag.Annee = Annee;
            ViewBag.isPagination = isPagination;
            ViewBag.pagId = pagId;
            
            ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            ViewBag.payDatas = JsonConvert.SerializeObject(payDatas);
            
            ViewBag.paginations = paginations;


            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(model);

        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Login" , "Admin");
        }
    }
    
    public async Task<JsonResult> NotifFacture()
    {
        int res = 0;
        string color = "text-danger";
        try
        {
            Admin admin = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
            Admin A = await _context.Admin.FindAsync(admin.Id);
            var VFI =  _context.VFacture.Where(q => q.EstPayer == null && q.DateSuspendue == null);
            
            if (!AdminService.IsLevel_5(A))
            {
                AffectationEmployer? affectation = await _context.AffectationEmployer
                    .Where(q => q.DateFin == null && q.IdAdmin == admin.Id)
                    .OrderByDescending(q => q.DateDeb)
                    .FirstOrDefaultAsync();
                VFI = VFI.Where(q => q.IdBoutique == affectation.IdBoutique);
            }
            
            var vfacts = await  VFI.CountAsync();
            
            
            if (vfacts < 1)
            {
                var vF = _context.VFacture
                    .Where(q => q.EstPayer != null && q.DateSuspendue == null && q.DatePret != null &&
                                q.DateLivrer == null);
                if (!AdminService.IsLevel_5(A))
                {
                    AffectationEmployer? affectation = await _context.AffectationEmployer
                        .Where(q => q.DateFin == null && q.IdAdmin == admin.Id)
                        .OrderByDescending(q => q.DateDeb)
                        .FirstOrDefaultAsync();
                    vF = vF.Where(q => q.IdBoutique == affectation.IdBoutique);
                }
                vfacts = await vF.CountAsync();
                color = "text-warning";
            }
            if (vfacts < 1)
            {
                var vF = _context.VFacture.Where(q =>
                    q.EstPayer != null && q.DateSuspendue == null && q.DatePret == null);
                if (!AdminService.IsLevel_5(A))
                {
                    AffectationEmployer? affectation = await _context.AffectationEmployer
                        .Where(q => q.DateFin == null && q.IdAdmin == admin.Id)
                        .OrderByDescending(q => q.DateDeb)
                        .FirstOrDefaultAsync();
                    vF = vF.Where(q => q.IdBoutique == affectation.IdBoutique);
                }
                vfacts = await vF
                    .CountAsync();
                color = "text-warning";
            }
            res += vfacts;
            return Json( new {res , color});
        }
        catch (Exception e)
        {
            return Json( new {res , color});
        }
    }
}