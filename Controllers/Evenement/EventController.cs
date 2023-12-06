using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Evenements.others;
using originalstoremada.Models.Evenements.views;
using originalstoremada.Services;
using originalstoremada.Services.Events;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Evenement;

public class EventController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public EventController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET
    public async Task<IActionResult> Calendrier(DateTime? date)
    {
        var calendar = await EventAdminService.GetCalendar(_context , date);
        
        List<SelectListItem> selectList = UtilesFonctions.Mois()
            .Select((moisValue, index) => new SelectListItem
            {
                Text = moisValue,
                Value = (index + 1).ToString()
            })
            .ToList();
        SelectList moisSelectList = new SelectList(selectList, "Value", "Text", calendar.Date.Month);
        ViewBag.mois = moisSelectList;
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View(calendar);
    }
    
    public  IActionResult NextCalendar(DateTime? Date)
    {
        
        if (Date != null)
        {
            DateTime d = (DateTime)Date;
            d = new DateTime(d.Year, d.Month, 1, d.Hour, d.Minute, d.Second);
            d = d.AddMonths(1);
            Date = d;
        }
        return RedirectToAction(nameof(Calendrier) , new { date = Date });
    }
    
    public  IActionResult PreviousCalendar(DateTime? Date)
    {
        
        if (Date != null)
        {
            DateTime d = (DateTime)Date;
            d = new DateTime(d.Year, d.Month, 1, d.Hour, d.Minute, d.Second);
            d = d.AddMonths(-1);
            Date = d;
        }
        return RedirectToAction(nameof(Calendrier) , new { date = Date });
    }

    public IActionResult FindCalendar(int mois , int year)
    {
        DateTime? Date = null;
        try
        {
            Date = new DateTime(year, mois, 1);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }    
        return RedirectToAction(nameof(Calendrier) , new { date = Date });
    }

    public async Task<IActionResult> Details(long id_event, int? pagId)
    {
        try
        {
            DateTime now = DateTime.Now;
            var elements = EventAdminService.GetEvents(_context, id_event);
            Pagination<VProduitEventReste> paginations = new Pagination<VProduitEventReste>(10, pagId, elements);
            elements= paginations.Paginate();
            var prods = await elements.ToListAsync();
            if (!prods.Any()) throw new Exception("Pas encore d'artcle disponible");
            await EventAdminService.SetPrixAriary(_context, prods);
            ViewBag.paginations = paginations;
            var events = await _context.Evenement.FindAsync(id_event);
            ViewBag.events = events;
            ViewBag.isTerminer = now > events.DateFin;
            ViewBag.isEnCours = now > events.DateDeb && now < events.DateFin;
            ViewData["categories"] = new SelectList (_context.CategorieProduit, "Id", "Nom");
        
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
        
            return View(prods);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Calendrier));
        }
        
    }

    public async Task<IActionResult> Carts()
    {
        List<VProduitEventReste> prods = new List<VProduitEventReste>();
        try
        {
            DateTime now = DateTime.UtcNow;
            var events = await _context.Evenement
                .FirstOrDefaultAsync( q => q.DateDeb < now && q.DateFin > now);

            if (events==null)
            {
                if(Request.Cookies[KeyStorage.EventCarts]!=null) Response.Cookies.Delete(KeyStorage.EventCarts);
                throw new Exception("Pas d`évènement en cours");
            }
            if(Request.Cookies[KeyStorage.EventCarts] == null) throw new Exception("Pas de panier");
            prods = JsonConvert.DeserializeObject<List<VProduitEventReste>>(Request.Cookies[KeyStorage.EventCarts]);
            await EventAdminService.SetPrixAriary(_context,prods);
            double[] sommeCarts = EventService.SommeCart(prods);

            ViewBag.events = events;
            ViewBag.sommeCarts = sommeCarts;
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
        }
        catch (Exception e)
        {
            if(Request.Cookies[KeyStorage.EventCarts]!=null) Response.Cookies.Delete(KeyStorage.EventCarts);
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Calendrier));
        }
        
        return View(prods);
    }

    [CheckClient]
    public async Task<IActionResult> AddToCart(long IdEvent,long IdproduitEvent, int Quantiter)
    {
        try
        {
            await EventService.AddToCart(_context, HttpContext, IdEvent,IdproduitEvent, Quantiter);
        }
        catch (Exception e)
        {
            // ignored
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Details), new { id_event = IdEvent });
        }
        return RedirectToAction(nameof(Carts));
    }
    
    [CheckClient]
    public async Task<IActionResult> UpdateToCart(long IdEvent,long IdproduitEvent, int Quantiter)
    {
        try
        {
            await EventService.UpdateCart(_context, HttpContext, IdEvent,IdproduitEvent, Quantiter);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(Carts));
    }
    
    [CheckClient]
    public async Task<IActionResult> DeleteToCarts(long IdproduitEvent)
    {
        try
        {
            EventService.DeleteCarts(HttpContext, IdproduitEvent);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(Carts));
    }
    
    [CheckClient]
    public async Task<IActionResult> ToDevis(long IdEvent)
    {
        try
        {
            long id_devis = await EventService.ToDevis(_context,HttpContext, IdEvent);
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return RedirectToAction("Details", "Devis" , new { id_devis });
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(Carts));
    }

    public async Task<JsonResult> EventExist()
    {
        int res = 0;
        string color = "text-danger";
        try
        {
            var events = await _context.ProduitEvent.Include(q => q.Evenement)
                .FirstOrDefaultAsync(
                    q => DateTime.UtcNow > q.Evenement.DateDeb && DateTime.UtcNow < q.Evenement.DateFin);
            res = events != null? 1 : 0;
            return Json( new {res , color});
        }
        catch (Exception e)
        {
            return Json( new {res , color});
        }
    }

}