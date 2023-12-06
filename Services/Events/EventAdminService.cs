using Microsoft.EntityFrameworkCore;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Evenements;
using originalstoremada.Models.Evenements.others;
using originalstoremada.Models.Evenements.views;

namespace originalstoremada.Services.Events;

public class EventAdminService
{

    public static  IQueryable<Evenement> AllEvents(ApplicationDbContext context)
    {
        IQueryable<Evenement> events =  context.Evenement.OrderByDescending(q => q.DateDeb);
        return events;
    }

    public static async Task<DateTime> CreateEvent(ApplicationDbContext context, DateOnly Date, TimeOnly heureDeb, TimeOnly heureFin , string Description)
    {
        DateTime dateDeb = new DateTime(Date.Year, Date.Month, Date.Day, heureDeb.Hour, heureDeb.Minute, heureDeb.Second);
        DateTime dateFin = new DateTime(Date.Year, Date.Month, Date.Day, heureFin.Hour, heureFin.Minute, heureFin.Second);
        DateTime now = DateTimeToUTC.Make(DateTime.Now);
        
        if (dateDeb.AddHours(1) < now) throw new Exception("Date Non cohérent");
        if (dateDeb > dateFin) throw new Exception("Date Non cohérent");
        var events = await context.Evenement.Where(q =>
            q.DateDeb.Year == dateDeb.Year && q.DateDeb.Month == dateDeb.Month &&
            DateTimeToUTC.Make(dateDeb) <= q.DateFin).ToListAsync();
        
        if(events.Any()) throw new Exception("Un évènement déjà prévu dans l'heure.");

        Evenement evenement = new Evenement();
        evenement.DateDeb = DateTimeToUTC.Make(dateDeb);
        evenement.DateFin = DateTimeToUTC.Make(dateFin);
        evenement.Description = Description;
        
        context.Add(evenement);
        await context.SaveChangesAsync();
        return dateDeb;
    }
    
    public static IQueryable<VProduitEventReste> GetEvents(ApplicationDbContext context, long id_event)
    {
        IQueryable<VProduitEventReste> prods = context.VProduitEventReste
            .Include(q => q.CategorieProduit)
            .Where(q => q.IdEvenement == id_event);
        return prods;
    }

    public static async Task SetPrixAriary(ApplicationDbContext context, List<VProduitEventReste> produitEventRestes)
    {
        if (produitEventRestes.Any())
        {
            var devis = await context.CoursEuro.OrderByDescending(q => q.Date).FirstAsync();
            foreach (var p in (produitEventRestes))
            {
                p.PrixAriary = p.Prix * devis.MontantAriary;
            }
        }
    }

    public static async Task<List<VProduitEventReste>> GetElementsByIdEvent(ApplicationDbContext context , long id_event)
    {
        var res = await context.VProduitEventReste.Where(q => q.IdEvenement == id_event).ToListAsync();
        return res;
    }

    public static async Task<List<Evenement>> FillEventsElements(ApplicationDbContext context,
        List<Evenement> evenements)
    {
        foreach (var p in evenements)
        {
            p.VProduitEventReste = await GetElementsByIdEvent(context, p.Id);
        }

        return evenements;
    }

    public static async Task<CalendarMonth> GetCalendar(ApplicationDbContext context, DateTime? D)
    {
        if (D == null)
        {
            D = DateTimeToUTC.Make(DateTime.Now);
        }
        
        DateTime date = (DateTime)D;

        var year = date.Year;
        var month = date.Month;

        // Calculez le premier jour du mois (1 pour lundi, 7 pour dimanche)
        var startDay = (int)new DateTime(year, month, 1).DayOfWeek;
        startDay = startDay == 0 ? 7 : startDay; // Lundi commence à 1

        // Calculez le nombre de jours dans le mois
        var daysInMonth = DateTime.DaysInMonth(year, month);

        List<DayInMonth> dMs = new List<DayInMonth>();

        var events = await context.Evenement.Where(q => q.DateDeb.Month == month && q.DateDeb.Year == year).ToListAsync();
        
        DateTime now = DateTimeToUTC.Make(DateTime.Now);
        for (int i = 1; i <= daysInMonth; i++)
        {
            DayInMonth d = new DayInMonth();
            d.DayNumber = i;
            d.ExistEvent = 0;
            d.Evenements = new List<Evenement>();

            var EV = events.Where( q => DateTimeToUTC.Make(q.DateDeb).Day == i ).ToList();
            if (EV.Any())
            {
                bool OK = false;
                foreach (var e in EV)
                {
                    if (!OK)
                    {
                        if (DateTimeToUTC.Make(e.DateDeb) < now && DateTimeToUTC.Make(e.DateFin) > now)
                        {
                            d.ExistEvent = 1; // Event
                            OK = true;
                        }
                        else if(DateTimeToUTC.Make(e.DateFin) < now)
                        {
                            d.ExistEvent = 2; // Fin
                            OK = true;
                        } else if (DateTimeToUTC.Make(e.DateDeb) > now)
                        {
                            d.ExistEvent = 3; //Futur
                            OK = true;
                        }   
                    }

                    d.Evenements.Add(e);
                }
            } else if (now.Year == year && now.Month == month && now.Day == i)
            {
                d.ExistEvent = -1;
            }
            
            dMs.Add(d);
        }

        var calendarModel = new CalendarMonth
        {
            Date = date,
            StartDay = startDay,
            DaysInMonths = dMs
        };
        
        return calendarModel;
    }
}