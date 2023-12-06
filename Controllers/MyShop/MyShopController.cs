using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.Users;
using originalstoremada.Services;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Shop;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace originalstoremada.Controllers.MyShop;

public class MyShopController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public MyShopController(ApplicationDbContext context)
    {
        _context = context;
    }

    [CheckClient]
    public async Task<IActionResult> Facture(int? pagId, int etat)
    {
        var facts = new List<VFacture>();
        try
        {
            Dictionary<string, object> MyFACTS = await MyShopService.MyFacture(_context, HttpContext, 10, pagId, etat);
            ViewBag.pagination = MyFACTS["pagination"];
            facts = (List<VFacture>)MyFACTS["myFacture"];
            ViewBag.etat = etat;
        }
        catch (Exception e)
        {
            // ignored
        }

        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View(facts);
    }

    [CheckClient]
    public async Task<IActionResult> DetailsFacture(long id_facture = 0)
    {
        List<VPayementProduit> payementProduits = new List<VPayementProduit>();
        try
        {
            payementProduits = await MyShopService.FactureDetails(_context, id_facture);
            var vFacture = await _context.VFacture
                .Include(q => q.Client)
                .Include(q => q.Boutique)
                .FirstOrDefaultAsync(q => q.Id == id_facture);
            MyShopService.SetEtatFacture(vFacture);
            ViewBag.facture = vFacture;
            ViewBag.suivisFacture = MyShopService.SuivisFactureEtat(vFacture);

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
    
    [CheckClient]
    public async Task<IActionResult> DetailsFacturePdf(long id_facture = 0)
    {
        List<VPayementProduit> payementProduits = new List<VPayementProduit>();
        try
        {
            payementProduits = await MyShopService.FactureDetails(_context, id_facture);
            var vFacture = await _context.VFacture
                .Include(q => q.Client)
                .Include(q => q.Boutique)
                .FirstOrDefaultAsync(q => q.Id == id_facture);
            
            MyShopService.SetEtatFacture(vFacture);
            var suivisFactureEtat = MyShopService.SuivisFactureEtat(vFacture);

            TypePayement typePayementForm = null;
            
            if (vFacture.EstPayer == null) typePayementForm = await _context.TypePayement.FirstOrDefaultAsync(q => q.Id == vFacture.IdTypePayement);
            var adresseLivraisonFacture = await _context.AdresseLivraisonFacture.FirstOrDefaultAsync(q => q.IdFacture == id_facture);

            var nums = await _context.TypePayement.ToListAsync();
            
            var model = new
            {
                payementProduits,
                facture = vFacture,
                suivisFacture = suivisFactureEtat,
                adresseLivraison = adresseLivraisonFacture,
                typePayementForm ,
                nums,
                now = DateTime.Now
            };
            
            // return View(model);
            
            var pdfResult = new ViewAsPdf("DetailsFacturePdf", model);

            pdfResult.FileName = $"facture-#{Formattage.Numero(id_facture)}.pdf";
        
            // pdfResult.PageOrientation = Orientation.Landscape;
            pdfResult.PageSize = Rotativa.AspNetCore.Options.Size.A4;
            
            pdfResult.ContentDisposition = ContentDisposition.Attachment;

            return pdfResult;
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Facture));
        }
    }

    [CheckClient()]
    public async Task<IActionResult> MesPayement(int? pagId, bool isPagination = false, int etat = 0)
    {
        try
        {
            if (etat > 2) etat = 0;
            if (!isPagination) pagId = 1;
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);

            IQueryable<PayementProduit> queryPayd = _context.PayementProduit
                .Include(q => q.TypePayement)
                .Include(q => q.Facture)
                .ThenInclude(q => q.Client)
                .OrderByDescending(q => q.Date)
                .Where(q => q.Facture.IdClient == client.Id);

            if (etat == 1) queryPayd = queryPayd.Where(q => q.Facture.EstPayer != null);
            else if (etat == 2) queryPayd = queryPayd.Where(q => q.Facture.EstPayer == null);

            Pagination<PayementProduit> paginations = new Pagination<PayementProduit>(10, pagId, queryPayd);
            paginations.Paginate();
            
            var model = await queryPayd.ToListAsync();
            
            ViewBag.isPagination = isPagination;
            ViewBag.pagId = pagId;
            ViewBag.etat = etat;


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
            return RedirectToAction(nameof(Facture));
        }
    }
    
    public async Task<JsonResult> NotifFacture()
    {
        int res = 0;
        string color = "text-danger";
        try
        {
            if (Request.Cookies[KeyStorage.KeyClient] == null) throw new Exception("Pas de compte");
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
            var vfacts = await _context.VFacture.Where(q => q.EstPayer == null && q.DateSuspendue == null && q.IdClient == client.Id)
                .CountAsync();
            if (vfacts < 1)
            {
                vfacts = await _context.VFacture
                    .Where(q => q.EstPayer != null && q.DateSuspendue == null && q.DatePret != null && q.IdClient == client.Id && q.DateLivrer == null)
                    .CountAsync();
                color = "text-success";
            }
            if (vfacts < 1)
            {
                vfacts = await _context.VFacture
                    .Where(q => q.EstPayer != null && q.DateSuspendue == null && q.DatePret == null && q.IdClient == client.Id)
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