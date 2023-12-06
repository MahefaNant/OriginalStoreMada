using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.StatVente;
using originalstoremada.Models.Users;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.Stats;

public class StatVenteController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public StatVenteController(ApplicationDbContext context)
    {
        _context = context;
    }

    [CheckAdminAll]
    public async Task<IActionResult> VenteProduit(int? mois, int? annee, int IdBoutique = 0, int top = 10)
    {
        try
        {
            DateTime now = DateTime.Now;

            ViewBag.now = now;

            if (mois == null) mois = now.Month;
            if (annee == null) annee = now.Year;

            ViewBag.mois = mois;
            ViewBag.annee = annee;
            ViewBag.top = top;

            List<SelectListItem> selectList = UtilesFonctions.Mois()
                .Select((moisValue, index) => new SelectListItem
                {
                    Text = moisValue,
                    Value = (index + 1).ToString()
                })
                .ToList();
            SelectList moisSelectList = new SelectList(selectList, "Value", "Text", mois);
            ViewBag.selectMois = moisSelectList;

            var BS = new List<Boutique>()
            {
                new() { Id = 0, Ville = "Tous", Quartier = "" }
            };
            var boutiques = await _context.Boutique.ToListAsync();
            BS.AddRange(boutiques);

            var selectListItems = BS.Select(vq => new SelectListItem
            {
                Value = vq.Id.ToString(),
                Text = $"{vq.Ville} - {vq.Quartier}"
            }).ToList();
            
            var selectListBout = new SelectList(selectListItems, "Value", "Text" , IdBoutique);
            ViewBag.selectListBout = selectListBout;
            
            Admin admin = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
            admin = await _context.Admin.FindAsync(admin.Id);
            if (AdminService.IsLevel_5(admin))
            {
                ViewBag.super = true;
            }

            if (AdminService.IsLevel_5(admin) && IdBoutique == 0)
            {
                List<int> valGlobal = new List<int>();
                List<string> labelGlobal = new List<string>();
                var global = _context.VCountBestSeller
                    .Include(q => q.Produit)
                    .OrderByDescending(q => q.TotalQuantiter)
                    .Take(top)
                    .ToList();
                foreach (var g in global)
                {
                    valGlobal.Add(g.TotalQuantiter);
                    labelGlobal.Add(g.Produit.Nom);
                }
                ViewBag.valGlobal = JsonConvert.SerializeObject(valGlobal);
                ViewBag.labelGlobal = JsonConvert.SerializeObject(labelGlobal);
                
                List<int> valAns = new List<int>();
                List<string> labelAns = new List<string>();
                var Ans = _context.VCountBestSellerParAns
                    .Include(q => q.Produit)
                    .OrderByDescending(q => q.TotalQuantiter)
                    .Where(q => q.Annee == annee)
                    .Take(top)
                    .ToList();

                foreach (var g in Ans)
                {
                    valAns.Add(g.TotalQuantiter);
                    labelAns.Add(g.Produit.Nom);
                }
                
                ViewBag.valAns = JsonConvert.SerializeObject(valAns);
                ViewBag.labelAns = JsonConvert.SerializeObject(labelAns);
                
                List<int> valMois = new List<int>();
                List<string> labelMois = new List<string>();
                var Mois = _context.VCountBestSellerParAnsMois
                    .Include(q => q.Produit)
                    .OrderByDescending(q => q.TotalQuantiter)
                    .Where(q => q.Annee == annee && q.Mois == mois)
                    .Take(top)
                    .ToList();

                foreach (var g in Mois)
                {
                    valMois.Add(g.TotalQuantiter);
                    labelMois.Add(g.Produit.Nom);
                }
                
                ViewBag.valMois = JsonConvert.SerializeObject(valMois);
                ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            } 

            else
            {
                List<int> valGlobal = new List<int>();
                List<string> labelGlobal = new List<string>();
                IQueryable<VCountBestSellerParBoutique> globalQuery = _context.VCountBestSellerParBoutique
                    .Include(q => q.Produit)
                    .OrderByDescending(q => q.TotalQuantiter);

                IQueryable<VCountBestSellerParAnsBoutique> AnsQuery = _context.VCountBestSellerParAnsBoutique
                    .Include(q => q.Produit)
                    .OrderByDescending(q => q.TotalQuantiter)
                    .Where(q => q.Annee == annee);

                IQueryable<VCountBestSellerParAnsMoisBoutique> MoisQuery = _context.VCountBestSellerParAnsMoisBoutique
                    .Include(q => q.Produit)
                    .OrderByDescending(q => q.TotalQuantiter)
                    .Where(q => q.Annee == annee && q.Mois == mois);

                if (AdminService.IsLevel_5(admin))
                {
                    globalQuery = globalQuery.Where(q => q.IdBoutique == IdBoutique);
                    AnsQuery = AnsQuery.Where(q => q.IdBoutique == IdBoutique);
                    MoisQuery = MoisQuery.Where(q => q.IdBoutique == IdBoutique);
                }
                else
                {
                    var affect = await _context.AffectationEmployer.FirstOrDefaultAsync(q => q.IdAdmin == admin.Id && q.DateFin == null);
                    globalQuery = globalQuery.Where(q => q.IdBoutique == affect.IdBoutique);
                    AnsQuery =AnsQuery.Where(q => q.IdBoutique == affect.IdBoutique);
                    MoisQuery = MoisQuery.Where(q => q.IdBoutique == affect.IdBoutique);
                }
                
                var global = globalQuery
                    .Take(top)
                    .ToList();
                foreach (var g in global)
                {
                    valGlobal.Add(g.TotalQuantiter);
                    labelGlobal.Add(g.Produit.Nom);
                }
                ViewBag.valGlobal = JsonConvert.SerializeObject(valGlobal);
                ViewBag.labelGlobal = JsonConvert.SerializeObject(labelGlobal);
                
                
                List<int> valAns = new List<int>();
                List<string> labelAns = new List<string>();
                var Ans = AnsQuery
                    .Take(top)
                    .ToList();

                foreach (var g in Ans)
                {
                    valAns.Add(g.TotalQuantiter);
                    labelAns.Add(g.Produit.Nom);
                }
                
                ViewBag.valAns = JsonConvert.SerializeObject(valAns);
                ViewBag.labelAns = JsonConvert.SerializeObject(labelAns);
                
                List<int> valMois = new List<int>();
                List<string> labelMois = new List<string>();
                var Mois = MoisQuery
                    .Take(top)
                    .ToList();

                foreach (var g in Mois)
                {
                    valMois.Add(g.TotalQuantiter);
                    labelMois.Add(g.Produit.Nom);
                }
                
                ViewBag.valMois = JsonConvert.SerializeObject(valMois);
                ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            }



            return View();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Home", "Admin");
        }
    }
    
}