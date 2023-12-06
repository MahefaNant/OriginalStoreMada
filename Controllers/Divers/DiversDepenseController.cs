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
using originalstoremada.Models.Users;
using originalstoremada.Services;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Divers;

public class DiversDepenseController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public DiversDepenseController(ApplicationDbContext context)
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

            IQueryable<DiversDepense> queryDD = _context.DiversDepense
                .Include(q => q.TypeDepense)
                .OrderByDescending(q => q.Date)
                .Where(q => q.Date.Year == Annee);

            Pagination<DiversDepense> paginations = new Pagination<DiversDepense>(10, pagId, queryDD);
            queryDD = paginations.Paginate();

            var depenses = await queryDD.ToListAsync();
            
            
            List<double> depenseParAnsMois = new List<double>();
            List<string> labelMois = new List<string>();

            List<double> depenseParType = new List<double>();
            List<string> labelTypes = new List<string>();

            List<VDiversDepenseParAnsMois> ddams = await _context.VDiversDepenseParAnsMois
                .Where(q => q.Annee == Annee).ToListAsync();
            
            foreach (var d in ddams)
            {
                labelMois.Add(UtilesFonctions.Mois()[d.Mois - 1]);
                depenseParAnsMois.Add(d.TotalMontant);
            }

            List<VDiversDepenseParAnsType> ddpat = await _context.VDiversDepenseParAnsType
                .Include(q => q.TypeDepense)
                .Where(q => q.Annee == Annee).ToListAsync();
            
            foreach (var d in ddpat)
            {
                labelTypes.Add(d.TypeDepense.Nom);
                depenseParType.Add(d.TotalMontant);
            }

            List<TypeDepense> typeDepenses = await _context.TypeDepense.ToListAsync();

            ViewBag.typeDepenses = typeDepenses;

            ViewBag.Annee = Annee;
            ViewBag.isPagination = isPagination;
            ViewBag.pagId = pagId;
            
            ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            ViewBag.depenseParAnsMois = JsonConvert.SerializeObject(depenseParAnsMois);
            
            ViewBag.labelTypes = JsonConvert.SerializeObject(labelTypes);
            ViewBag.depenseParType = JsonConvert.SerializeObject(depenseParType);

            ViewBag.paginations = paginations;


            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(depenses);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Login" , "Admin");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(DiversDepense diversDepense, int? Annee , int? pagId, bool isPagination = false)
    {
        try
        {
            var dep = await _context.DiversDepense.FindAsync(diversDepense.Id);

            if (dep != diversDepense)
            {
                dep.IdTypeDepense = diversDepense.IdTypeDepense;
                dep.Date = DateTimeToUTC.Make(diversDepense.Date);
                dep.Corps = diversDepense.Corps;
                dep.MontantAr = diversDepense.MontantAr;
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
    public async Task<IActionResult> UpdateType(TypeDepense typeDepense, int? Annee , int? pagId, bool isPagination = false)
    {
        try
        {
            var dep = await _context.TypeDepense.FindAsync(typeDepense.Id);

            if (dep != typeDepense)
            {
                dep.Nom = typeDepense.Nom;
                dep.Code = typeDepense.Code;
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
    public IActionResult Add(DiversDepense diversDepense)
    {
        try
        {
            diversDepense.Date = DateTimeToUTC.Make(diversDepense.Date );
            _context.Add(diversDepense);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valider!";
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public IActionResult AddType(TypeDepense typeDepense)
    {
        try
        {
            _context.Add(typeDepense);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            TempData["error"] = "Information non valider!";
        }
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Remove(long IdDepense)
    {
        try
        {
            var dep = await _context.DiversDepense.FindAsync(IdDepense);
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
            var dep = await _context.TypeDepense.FindAsync(IdType);
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
        return await CsvController.DownloadCsv(C, "model-depense");
    }

    [HttpPost]
    public async Task<IActionResult> ImportDepenseFromCsv(IFormFile fichier)
    {
        try
        {
            using var reader = new StreamReader(fichier.OpenReadStream());

            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });     
        
            var divers = csv.GetRecords<DiversCsv>().ToList();

            var type = await _context.TypeDepense.ToListAsync();

            foreach (var D in divers)
            {
                try
                {
                    D.Type = D.Type.ToUpper();
                    D.Montant = D.Montant.Replace(".", ",");

                    TypeDepense T = type.FirstOrDefault(q => q.Code.ToUpper() == D.Type || q.Nom.ToUpper() == D.Type);
                    if (T != null)
                    {
                        DiversDepense DS = new DiversDepense()
                        {
                            IdTypeDepense= T.Id, MontantAr = Convert.ToDouble(D.Montant),
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