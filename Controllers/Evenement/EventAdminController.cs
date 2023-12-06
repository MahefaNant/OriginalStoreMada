using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Evenements;
using originalstoremada.Models.Evenements.views;
using originalstoremada.Services;
using originalstoremada.Services.Events;

namespace originalstoremada.Controllers.Evenement;

public class EventAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    public readonly IWebHostEnvironment webHostEnvironment;

    public EventAdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        this.webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(int? pagId)
    {
        IQueryable<Models.Evenements.Evenement> eventsQ = EventAdminService.AllEvents(_context);
        Pagination<Models.Evenements.Evenement> paginations = new Pagination<Models.Evenements.Evenement>(5, pagId, eventsQ);
        eventsQ = paginations.Paginate();
        List<Models.Evenements.Evenement> events = await eventsQ.ToListAsync();
        events = await EventAdminService.FillEventsElements(_context, events);
        ViewBag.paginations = paginations;
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View(events);
    }

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

    [HttpPost]
    public async Task<IActionResult> CreateEvent(DateOnly Date, TimeOnly heureDeb, TimeOnly heureFin , string Description)
    {
        try
        {
            DateTime dateTime = await EventAdminService.CreateEvent(_context, Date, heureDeb, heureFin, Description);
            return RedirectToAction(nameof(Calendrier) , new { date =  dateTime });
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        DateTime date = new DateTime(Date.Year, Date.Month, Date.Day, heureDeb.Hour, heureDeb.Minute, heureDeb.Second);
        return RedirectToAction(nameof(Calendrier) , new { date });
    }
    

    [HttpPost]
    public async Task<IActionResult> UpdateEvent(Models.Evenements.Evenement evenement)
    {
        try
        {
            if (evenement.Id <= 0) throw new Exception("Une erreur rencontrée");
            evenement.DateDeb = DateTimeToUTC.Make(evenement.DateDeb);
            evenement.DateFin = DateTimeToUTC.Make(evenement.DateFin);
            var ev = await _context.Evenement.FindAsync(evenement.Id);
            ev.DateDeb = DateTimeToUTC.Make(evenement.DateDeb);
            ev.DateFin = DateTimeToUTC.Make(evenement.DateFin);
            if (ev != evenement)
            {
                if (evenement.DateDeb.AddMinutes(2) < DateTimeToUTC.Make(ev.DateDeb)) throw new Exception("Date Non cohérent");
                if (evenement.DateDeb > evenement.DateFin) throw new Exception("Date Non cohérent");
                ev.DateDeb = evenement.DateDeb;
                ev.DateFin = evenement.DateFin;
                ev.Description = evenement.Description;
                _context.Update(ev);
                await _context.SaveChangesAsync();   
            }
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AddProduitEvent(long id_event, int? pagId)
    {
        DateTime now = DateTime.Now;
        var elements = EventAdminService.GetEvents(_context, id_event);
        Pagination<VProduitEventReste> paginations = new Pagination<VProduitEventReste>(10, pagId, elements);
        paginations.Paginate();
        var prods = await elements.ToListAsync();
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

    [HttpPost]
    public IActionResult AddProduitEvent(ProduitEvent produitEvent, List<IFormFile> Images)
    {
        try
        {
            if (!produitEvent.Couleur.IsNullOrEmpty() && produitEvent.Prix > 0 && !produitEvent.Taille.IsNullOrEmpty() &&
                !produitEvent.ProduitName.IsNullOrEmpty()
                && produitEvent.QuantiterMax > 0 && Images.Any())
            {
                Guid guid = Guid.NewGuid();
                List<string> savedFilesNames = ImageService.UploadAndResizeImages(webHostEnvironment,guid,Images,"images/produits" ,1200, 1486, false);
                ImageService.UploadAndResizeImages(webHostEnvironment,guid, Images,"images/produits" ,120, 120, true);
                produitEvent.Image = savedFilesNames[0];
                _context.Add(produitEvent);
                _context.SaveChanges();
            }
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(AddProduitEvent) , new { id_event = produitEvent.IdEvenement });
    }
    
    
}