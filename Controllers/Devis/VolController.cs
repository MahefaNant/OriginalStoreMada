using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Devis;
using originalstoremada.Services;
using originalstoremada.Services.RedirectAttribute;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace originalstoremada.Controllers.Devis;
public class VolController : Controller
{
    private readonly ApplicationDbContext _context;

    public VolController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Vol
    [CheckAdminAll]
    public async Task<IActionResult> Index(int? pagId)
    {
        try
        {
            IQueryable<Vol> query = _context.Vol.OrderByDescending(q =>  q.DateDepart);
            Pagination<Vol> paginations = new Pagination<Vol>(10, pagId, query);
            query = paginations.Paginate();
        
            var vol = await query.ToListAsync();
            ViewBag.paginations = paginations;
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(vol);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Home", "Admin");
        }
    }
    
    public async Task<IActionResult> PDF()
    {
        int? pagId = 1;
        IQueryable<Vol> query = _context.Vol.OrderByDescending(q => q.DateDepart);
        Pagination<Vol> paginations = new Pagination<Vol>(10, pagId, query);
        paginations.Paginate();

        var vol = await query.ToListAsync();
        ViewBag.paginations = paginations;

        // Créez le résultat ViewAsPdf
        var pdfResult = new ViewAsPdf("Index", vol);

        // Définissez le nom du fichier PDF généré
        pdfResult.FileName = "example.pdf";
        
        pdfResult.PageOrientation = Orientation.Portrait;
        pdfResult.PageSize = Rotativa.AspNetCore.Options.Size.A4;

        // Forcez le téléchargement du PDF plutôt que de l'afficher dans le navigateur
        pdfResult.ContentDisposition = ContentDisposition.Attachment;

        return pdfResult;
    }
    
    [CheckAdminAll]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateTime DateDepart, DateTime DateArriver)
    {
        try
        {
            var vol = new Vol()
            {
                DateDepart = DateTimeToUTC.Make(DateDepart), DateArriverEstimer = DateTimeToUTC.Make(DateArriver)
            };
            _context.Add(vol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["error"] = "Erreur d'insertion";
            return RedirectToAction(nameof(Index));
        }
       
    }
    
    [CheckAdminAll]
    [HttpPost]
    public async Task<IActionResult> Edit(Vol vol, long IdVol)
    {
        try
        {
            var V = await _context.Vol.FindAsync(IdVol);
            V.DateDepart = DateTimeToUTC.Make(vol.DateDepart);
            V.DateArriverEstimer = DateTimeToUTC.Make(vol.DateArriverEstimer);
            if(V.DateArriver!=null) V.DateArriver =  DateTimeToUTC.Make((DateTime)V.DateArriver);
           
            _context.Update(V);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = "Une erreur d'insertion";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [CheckAdminAll]
    [HttpPost]
    public async Task<IActionResult> SetArriver(long id_vol)
    {
        try
        {
            var vol = await _context.Vol.FindAsync(id_vol);
            vol.DateDepart = DateTimeToUTC.Make(vol.DateDepart);
            vol.DateArriverEstimer = DateTimeToUTC.Make(vol.DateArriverEstimer);
            vol.DateArriver = DateTimeToUTC.Make(DateTime.Now);

            _context.Update(vol);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}

