using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Controllers.CSV;
using originalstoremada.Data;
using originalstoremada.Models;
using originalstoremada.Models.CSV;
using originalstoremada.Models.StatAdmin;
using originalstoremada.Models.StatAdmin.ParAns.Plus;
using originalstoremada.Services;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Divers;

public class DiversEntrerController : Controller
{
    private readonly ApplicationDbContext _context;

    public DiversEntrerController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [CheckAdminLevel5]
    public async Task<IActionResult> Index(int? Annee , int? pagId, bool isPagination = false)
    {
        try
        {
            if (Annee == null)
            {
                DateTime D = DateTimeToUTC.Make(DateTime.Now);
                Annee = D.Year;
            }

            if (!isPagination) pagId = 1;

            IQueryable<DiversEntrer> queryDD = _context.DiversEntrer
                .Include(q => q.TypeEntrer)
                .OrderByDescending(q => q.Date)
                .Where(q => q.Date.Year == Annee);

            Pagination<DiversEntrer> paginations = new Pagination<DiversEntrer>(10, pagId, queryDD);
            queryDD = paginations.Paginate();

            var entrers = await queryDD.ToListAsync();

            List<double> entrerParAnsMois = new List<double>();
            List<string> labelMois = new List<string>();

            List<double> entrerParType = new List<double>();
            List<string> labelTypes = new List<string>();

            List<VDiversentrerParAnsMois> ddams = await _context.VDiversentrerParAnsMois
                .Where(q => q.Annee == Annee).ToListAsync();
            
            foreach (var d in ddams)
            {
                labelMois.Add(UtilesFonctions.Mois()[d.Mois -1]);
                entrerParAnsMois.Add(d.TotalBenefice);
            }

            List<VDiversEntrerParAnsType> ddpat = await _context.VDiversEntrerParAnsType
                .Include(q => q.TypeEntrer)
                .Where(q => q.Annee == Annee).ToListAsync();
            
            foreach (var d in ddpat)
            {
                labelTypes.Add(d.TypeEntrer.Nom);
                entrerParType.Add(d.TotalBenefice);
            }

            List<TypeEntrer> typeEntrer = await _context.TypeEntrer.ToListAsync();

            ViewBag.typeEntrer = typeEntrer;

            ViewBag.Annee = Annee;
            ViewBag.isPagination = isPagination;
            ViewBag.pagId = pagId;
            
            ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            ViewBag.entrerParAnsMois = JsonConvert.SerializeObject(entrerParAnsMois);
            
            ViewBag.labelTypes = JsonConvert.SerializeObject(labelTypes);
            ViewBag.entrerParType = JsonConvert.SerializeObject(entrerParType);

            ViewBag.paginations = paginations;


            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(entrers);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Login" , "Admin");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(DiversEntrer diversEntrer, int? Annee , int? pagId, bool isPagination = false)
    {
        try
        {
            var dep = await _context.DiversEntrer.FindAsync(diversEntrer.Id);

            if (dep != diversEntrer)
            {
                dep.IdTypeEntrer = diversEntrer.IdTypeEntrer;
                dep.Date = DateTimeToUTC.Make(diversEntrer.Date);
                dep.Corps = diversEntrer.Corps;
                dep.MontantAr = diversEntrer.MontantAr;
                _context.Update(dep);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valide!";
        }

        return RedirectToAction(nameof(Index), new { Annee, pagId, isPagination });
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdateType(TypeEntrer typeEntrer, int? Annee , int? pagId, bool isPagination = false)
    {
        try
        {
            var dep = await _context.TypeEntrer.FindAsync(typeEntrer.Id);

            if (dep != typeEntrer)
            {
                dep.Nom = typeEntrer.Nom;
                dep.Code = typeEntrer.Code;
                _context.Update(dep);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            TempData["error"] = "Information Non valide!";
        }

        return RedirectToAction(nameof(Index), new { Annee, pagId, isPagination });
    }

    [HttpPost]
    public IActionResult Add(DiversEntrer diversEntrer)
    {
        try
        {
            Console.WriteLine(JsonConvert.SerializeObject(diversEntrer));
            diversEntrer.Date = DateTimeToUTC.Make(diversEntrer.Date );
            _context.Add(diversEntrer);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valider!";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public IActionResult AddType(TypeEntrer typeEntrer)
    {
        try
        {
            _context.Add(typeEntrer);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valider!";
        }
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Remove(long IdEntrer)
    {
        try
        {
            var dep = await _context.DiversEntrer.FindAsync(IdEntrer);
            _context.Remove(dep);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valider!";
        }
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> RemoveType(long IdType)
    {
        try
        {
            var dep = await _context.TypeEntrer.FindAsync(IdType);
            _context.Remove(dep);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valider!";
        }
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> DownCsvModel()
    {
        List<DiversCsv> C = new List<DiversCsv>()
        {
            new()
            {
                Type = "AUT", Montant = "2000.1", Corps = "description", 
                Date = DateTime.Now
            }
        };
        return await CsvController.DownloadCsv(C, "model-entrer");
    }

    [HttpPost]
    public async Task<IActionResult> ImportEntrerFromCsv(IFormFile fichier)
    {
        try
        {
            using var reader = new StreamReader(fichier.OpenReadStream());

            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });     
        
            var divers = csv.GetRecords<DiversCsv>().ToList();
            
            var type = await _context.TypeEntrer.ToListAsync();
            
            foreach (var D in divers)
            {
                try
                {
                    D.Type = D.Type.ToUpper();
                    D.Montant = D.Montant.Replace(".", ",");

                    TypeEntrer T = type.FirstOrDefault(q => q.Code.ToUpper() == D.Type || q.Nom.ToUpper() == D.Type);
                    if (T != null)
                    {
                        DiversEntrer DS = new DiversEntrer()
                        {
                            IdTypeEntrer = T.Id, MontantAr = Convert.ToDouble(D.Montant),
                            Corps = D.Corps, Date = DateTimeToUTC.Make(D.Date)
                        };
                        _context.Add(DS);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}