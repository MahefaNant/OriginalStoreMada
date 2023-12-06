using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Controllers.CSV;
using originalstoremada.Data;
using originalstoremada.Models.Boutique;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.CSV;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.views;
using originalstoremada.Models.Users;
using originalstoremada.Services;
using originalstoremada.Services.Produits;
using originalstoremada.Services.Produits.Others;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.Produits;
public class ProduitController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProduitController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Produit
    [CheckAdminAll]
    public async Task<IActionResult> Index(int? pagId, RechercheProduitAdmin? recherche, bool refresh = false, bool isPagination = false)
    {
        try
        {
            IQueryable<VImagePrincipalPrixProduit> produits = ProduitService.AllProduits(_context,HttpContext, recherche, refresh, isPagination);
            Pagination<VImagePrincipalPrixProduit> pagination = new Pagination<VImagePrincipalPrixProduit>(10, pagId, produits);
            produits = pagination.Paginate();
            Admin admin = AdminService.GetByCookies(Request.Cookies["admin"]) ;
            admin = await _context.Admin.FindAsync(admin.Id);
            if (AdminService.IsLevel_5(admin)) ViewBag.isLevel5 = true;
            ViewBag.paginations = pagination;
        
            if (HttpContext.Session.GetString(ProduitService.SessionNameRechercheAdmin) != null)
            {
                recherche = JsonConvert.DeserializeObject<RechercheProduitAdmin>(HttpContext.Session.GetString(ProduitService.SessionNameRechercheAdmin));
                ViewBag.recherche = recherche;
            }
        
            var allCategories = new List<CategorieProduit>() { new() { Id = 0, Nom = "Tous"} };
            allCategories.AddRange( await _context.CategorieProduit.ToListAsync());
            ViewBag.categories = new SelectList(allCategories, "Id", "Nom", recherche.IdCategorie);
        
            var allTypes = new List<TypeProduit>() { new() { Id = 0, Nom = "Tous"} };
            allTypes.AddRange( await _context.TypeProduit.ToListAsync());
            ViewBag.types = new SelectList(allTypes, "Id", "Nom", recherche.IdType);
            return View(await produits.ToListAsync());
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Home", "Admin");
        }

    }
    

    // GET: Produit/Create
    [CheckAdminAll]
    public async Task<IActionResult> Create(long? id_produit)
    {
        Admin admin = AdminService.GetByCookies(Request.Cookies["admin"]) ;
        admin = await _context.Admin.FindAsync(admin.Id);
        ViewBag.isBoss = AdminService.IsLevel_5(admin);
        Produit produit = null;
        if (id_produit != null && id_produit!=0)
        {
            ViewData["id_produit"] = id_produit;
            produit = await _context.Produit.FindAsync(id_produit);
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit, "Id", "Nom", produit.IdCategorie);
            ViewData["IdType"] = new SelectList(_context.TypeProduit, "Id", "Nom",produit.IdType);
        }
        else
        {
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit, "Id", "Nom");
            ViewData["IdType"] = new SelectList(_context.TypeProduit, "Id", "Nom");
        }
        return View(produit);
    }

    private bool ProduitExists(long id)
    {
      return (_context.Produit?.Any(e => e.Id == id)).GetValueOrDefault();
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAvancer([Bind("Id,IdCategorie,IdType,Nom,Description,Fournisseur, PourEnfant")]Produit produit, string? Achat, string? Vente)
    {
        try
        {
            if (produit.Id <= 0)
            {
                double achat = double.Parse(Achat.Replace(".", ","));
                double vente = double.Parse(Vente.Replace(".", ","));
                long id_produit = ProduitService.SaveAvancer(_webHostEnvironment, _context,produit, achat, vente);
                return RedirectToAction("Index","PreferenceProduit" , new { id_produit });
            }

            await ProduitService.UpdateProduit(_context, produit);
            return RedirectToAction("Index","PreferenceProduit" , new { id_produit = produit.Id });
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Create), new { id_produit = produit.Id!=0 ? produit.Id : 0 });
        }
    }

    [HttpPost]
    public IActionResult AjouterPrix(int id_produit, string Achat, string Vente)
    {
        try
        {
            double achat = double.Parse(Achat.Replace(".", ","));
            double vente = double.Parse(Vente.Replace(".", ","));
            var prix = new PrixProduit()
            {
                IdProduit = id_produit, PrixAchat = achat, PrixVente = vente,DateDeb = DateTimeToUTC.Make(DateTime.Now)
            };
            Console.WriteLine(JsonConvert.SerializeObject(prix));
            _context.Add(prix);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            TempData["error"] = "Une erreur est survenue";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [CheckAdminLevel5]
    public IActionResult AjouterPromo(int id_produit, string promo, DateTime dateDeb , DateTime dateFin)
    {
        try
        {
            double Pr = double.Parse(promo);

            if(Pr <=0) throw new Exception("Promotion impossible");
            if (dateDeb > dateFin) throw new Exception("Date non cohérant");

            var prom = new PromotionProduit()
            {
                IdProduit = id_produit , DateDeb = DateTimeToUTC.Make(dateDeb) , DateFin = DateTimeToUTC.Make(dateFin),Pourcenttage = Pr
            };
            
            _context.Add(prom);
            _context.SaveChanges();
            
            return RedirectToAction(nameof(Index) , new {refresh = true});
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index) , new {refresh = true});
        }
    }

    [HttpPost]
    public async Task<IActionResult> ImportProduitsFromCsv(IFormFile fichier)
    {
        try
        {
            await ProduitService.ImportProduitsFromCsv(_context, fichier);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }

        return RedirectToAction(nameof(Index), new { refresh = true });
    }
    
    public async Task<IActionResult> DownCsvModel()
    {
        List<ProduitsCSV> C = new List<ProduitsCSV>()
        {
            new()
            {
                Categorie = "CHA" , Genre = "M" , Nom = "Air Nike", Fournisseur = "xxx",
                PrixAchat = "1000", PrixVente = "2000" , Description = "description"
            }
        };
        return await CsvController.DownloadCsv(C, "model-produit");
    }

    [CheckAdminAll]
    public async Task<IActionResult> EntrerProduit(int? pagId ,  DateTime? dateDeb, DateTime? dateFin,int IdBoutique = 0, bool isPagination = false)
    {
        try
        {
            if (Request.Cookies[KeyStorage.KeyAdmin] == null) throw new Exception("Erreur fatal");
            Admin A = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
            Admin admin = await _context.Admin.FirstOrDefaultAsync(q => q.Id == A.Id);
            
            var E = _context.EntreeProduit
                .Include(q => q.Produit)
                .Include(q => q.Boutique)
                .Include(q => q.PreferenceProduit)
                .ThenInclude(q => q.ContenueProduit)
                .Where(q => true);

            if (!AdminService.IsLevel_5(admin))
            {
                AffectationEmployer? affectation = await _context.AffectationEmployer
                    .Where(q => q.DateFin == null && q.IdAdmin == admin.Id)
                    .OrderByDescending(q => q.DateDeb)
                    .FirstOrDefaultAsync();
                E = E.Where(q => q.IdBoutique == affectation.IdBoutique);

               
            }
            else
            {
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

                if (IdBoutique != 0)
                {
                    E = E.Where(q => q.IdBoutique == IdBoutique);
                }
                
                
                ViewBag.selectListBout = selectListBout;
                ViewBag.super = true;
            }

            if (!isPagination)
            {
                pagId = 1;

                if (dateDeb != null && dateFin != null)
                {
                    E = E.Where(q => q.Date >= DateTimeToUTC.Make((DateTime)dateDeb) && q.Date <= DateTimeToUTC.Make((DateTime)dateFin));
                }
            }

            Pagination<EntreeProduit> paginations = new Pagination<EntreeProduit>(4, pagId, E);
            E = paginations.Paginate();

            ViewBag.paginations = paginations;

            var res = await E.OrderByDescending(q => q.Date).ToListAsync();

            return View(res);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index));
        }
    }
    
    [CheckAdminAll]
    public async Task<IActionResult> SortieProduit(int? pagId ,  DateTime? dateDeb, DateTime? dateFin,int IdBoutique = 0, bool isPagination = false)
    {
        try
        {
            if (Request.Cookies[KeyStorage.KeyAdmin] == null) throw new Exception("Erreur fatal");
            Admin A = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
            Admin admin = await _context.Admin.FirstOrDefaultAsync(q => q.Id == A.Id);
            
            var E = _context.SortieProduit
                .Include(q => q.Produit)
                .Include(q => q.Boutique)
                .Include(q => q.PreferenceProduit)
                .ThenInclude(q => q.ContenueProduit)
                .Where(q => true);

            if (!AdminService.IsLevel_5(admin))
            {
                AffectationEmployer? affectation = await _context.AffectationEmployer
                    .Where(q => q.DateFin == null && q.IdAdmin == admin.Id)
                    .OrderByDescending(q => q.DateDeb)
                    .FirstOrDefaultAsync();
                E = E.Where(q => q.IdBoutique == affectation.IdBoutique);

               
            }
            else
            {
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

                if (IdBoutique != 0)
                {
                    E = E.Where(q => q.IdBoutique == IdBoutique);
                }
                
                
                ViewBag.selectListBout = selectListBout;
                ViewBag.super = true;
            }

            if (!isPagination)
            {
                pagId = 1;

                if (dateDeb != null && dateFin != null)
                {
                    E = E.Where(q => q.Date >= DateTimeToUTC.Make((DateTime)dateDeb) && q.Date <= DateTimeToUTC.Make((DateTime)dateFin));
                }
            }

            Pagination<SortieProduit> paginations = new Pagination<SortieProduit>(4, pagId, E);
            E = paginations.Paginate();

            ViewBag.paginations = paginations;

            var res = await E.OrderByDescending(q => q.Date).ToListAsync();

            return View(res);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index));
        }
    }

}
