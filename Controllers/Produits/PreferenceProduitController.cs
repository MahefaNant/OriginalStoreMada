using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Users;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.Produits
{
    public class PreferenceProduitController : Controller
    {
        private readonly ApplicationDbContext _context;
        public readonly IWebHostEnvironment webHostEnvironment;

        public PreferenceProduitController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        // GET: PreferenceProduit
        [CheckAdminAll]
        public async Task<IActionResult> Index(long id_produit, long? id_contenue)
        {
            try
            {
                
                Admin a = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
                
                Admin admin = await _context.Admin.FindAsync(a.Id);

                var niveauAdmin = AdminService.IsLevel_5(admin);
                
                ViewBag.niveauAdmin = niveauAdmin;
                
                var stocks = _context.VStockPreference
                    .Include(p => p.Boutique)
                    .Include(q => q.ContenueProduit)
                    .OrderBy(q => q.Taille)
                    .Where(q => q.IdProduit == id_produit);
                
                if (niveauAdmin)
                {
                    ViewBag.boutiques = await _context.Boutique.ToListAsync();
                }
                else
                {
                    var affect = await _context.AffectationEmployer
                        .Include(q => q.Boutique)
                        .OrderByDescending(q => q.DateDeb)
                        .FirstOrDefaultAsync(q => q.IdAdmin == a.Id);
                    stocks = stocks.Where(q => q.IdBoutique == null|| q.IdBoutique == affect.IdBoutique);

                    if (affect == null) throw new Exception("Une erreur est survenue!");

                    ViewBag.boutiquesAffect = affect.Boutique;
                }

                var contenues = await _context.ContenueProduit.Where(q => q.IdProduit == id_produit)
                    .OrderByDescending(q => q.IsPrincipal).ToListAsync();

                ContenueProduit contenueSelected;
                List<PreferenceProduit> preferenceProduits = new List<PreferenceProduit>();

                if (id_contenue == null)
                {
                    stocks = stocks.Where(q => q.ContenueProduit.IsPrincipal);
                    contenueSelected = contenues.FirstOrDefault(q => q.IsPrincipal);
                }
                else
                {
                    stocks = stocks.Where(q => q.IdContenue == id_contenue);
                    contenueSelected = contenues.FirstOrDefault(q => q.Id == id_contenue);
                }

                if (contenueSelected != null)
                {
                    preferenceProduits = await _context.PreferenceProduit.Where(q => q.IdContenue == contenueSelected.Id).ToListAsync();
                }
            
                ViewBag.id_produit = id_produit;
                ViewBag.produit = await _context.VImagePrincipalPrixProduit.FirstOrDefaultAsync(q => q.Id == id_produit);
                ViewBag.contenues = contenues;
                ViewBag.contenueSelected = contenueSelected;
                ViewBag.preferenceProduits = preferenceProduits;
                
            
                if (TempData.TryGetValue("error", out var error))
                {
                    ViewBag.error = error;
                }
                
                return View(await stocks.ToListAsync());
            }
            catch (Exception e)
            {
                TempData["error"] = "Une erreur Est survenue";
                return RedirectToAction("Index", "Produit" , new { refresh = true });
            }
        }
        
        public async Task<IActionResult> Create(long id_produit, List<IFormFile> Images, string Couleur , List<string> tailles)
        {
            try
            {
                if (id_produit <= 0) throw new Exception("le produit n`est pas mentionner");
                if(!Images.Any()) throw new Exception("Veuillez-choisir l`image");
                if(Couleur.IsNullOrEmpty()) throw new Exception("Veuillez préciser l`information de la couleur");
                if(!tailles.Any()) throw new Exception("Veuillez ajouter des tailles");
                
                var conts = await _context.ContenueProduit.Where(q => q.IdProduit == id_produit).ToListAsync();
                bool isPrincipal = !conts.Any();
                
                Guid guid = Guid.NewGuid();
                List<string> savedFilesNames = ImageService.UploadAndResizeImages(webHostEnvironment,guid,Images,"images/produits" ,1200, 1486, false);
                ImageService.UploadAndResizeImages(webHostEnvironment,guid, Images,"images/produits" ,120, 120, true);

                var contenueP = new ContenueProduit()
                {
                    IdProduit = id_produit, Couleur = Couleur , Image = savedFilesNames[0], IsPrincipal = isPrincipal
                };

                _context.Add(contenueP);

                await _context.SaveChangesAsync();

                foreach (var t in tailles)
                {
                    var pref = new PreferenceProduit()
                    {
                        IdProduit = id_produit, Taille = t, IdContenue = contenueP.Id
                    };
                    _context.Add(pref);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id_produit });
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
                return RedirectToAction(nameof(Index), new { id_produit });
            }
        }

        /*[CheckAdminAll]
        [HttpPost]
        public async Task<IActionResult> Create(int? IdBoutique ,long IdProduit ,long IdContenue, string Taille, int? Stock)
        {
            try
            {
                PreferenceProduit preferenceProduit = new PreferenceProduit()
                {
                    IdProduit = IdProduit, IdContenue = IdContenue, Taille = Taille
                };
                var pref = await _context.PreferenceProduit.FirstOrDefaultAsync(q =>
                    q.IdProduit == IdProduit && q.IdContenue == IdContenue && Taille.Contains(q.Taille));
                
                if (pref == null)
                {
                    _context.Add(preferenceProduit);
                    await _context.SaveChangesAsync();
                }
                if (Stock!=null && Stock > 0)
                {
                    Admin a = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
                    Admin admin = await _context.Admin.FindAsync(a.Id);
                    bool isBoss = AdminService.IsLevel_5(admin);
                    int id_boutique = 0;
                    if (isBoss) id_boutique = (int)IdBoutique;
                    else
                    {
                        var affect = await _context.AffectationEmployer.Where(q => q.IdAdmin == admin.Id)
                            .OrderByDescending(q => q.DateDeb).FirstAsync();
                        id_boutique = affect.IdBoutique;
                    }

                    var P = preferenceProduit;
                    if (pref != null) P = pref;
                    
                    EntreeProduit entreeProduit = new EntreeProduit()
                    {
                        IdProduit = P.IdProduit, IdPreferenceProduit = P.Id, IdBoutique = id_boutique,
                        Date = DateTimeToUTC.Make(DateTime.Now), Quantiter = (int)Stock
                    };
                    _context.Add(entreeProduit);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            return RedirectToAction(nameof(Index), new { id_produit = IdProduit });
        }*/

        [CheckAdminAll]
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int? IdBoutique, long IdProduit , long Idpref, int? Stock)
        {
            try
            {
                if (Stock is null or <= 0) throw new Exception("Quantiter de stock inférieur à 1");
                Admin a = JsonConvert.DeserializeObject<Admin>(Request.Cookies[KeyStorage.KeyAdmin]);
                Admin admin = await _context.Admin.FindAsync(a.Id);
                bool isBoss = AdminService.IsLevel_5(admin);

                int id_boutique = 0;
                if (isBoss) id_boutique = (int)IdBoutique;
                else
                {
                    var affect = await _context.AffectationEmployer.Where(q => q.IdAdmin == admin.Id)
                        .OrderByDescending(q => q.DateDeb).FirstAsync();
                    id_boutique = affect.IdBoutique;
                }

                var entree = new EntreeProduit()
                {
                    IdBoutique = id_boutique, IdProduit = IdProduit, IdPreferenceProduit = Idpref,Quantiter = (int)Stock, 
                    Date = DateTimeToUTC.Make(DateTime.Now)
                };
                _context.Add(entree);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }

            return RedirectToAction(nameof(Index), new { id_produit = IdProduit });
        }
        
        [CheckAdminAll]
        [HttpPost]
        public async Task<IActionResult> AddTaille(long id_produit, long id_contenue, List<string> tailles)
        {
            try
            {
                if (!tailles.Any()) throw new Exception("Valeur vide");

                foreach (var q in tailles)
                {
                    var pref = new PreferenceProduit()
                    {
                        IdProduit = id_produit, IdContenue = id_contenue,Taille = q
                    };
                    _context.Add(pref);
                }

                if (tailles.Any()) await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index), new { id_produit = id_produit, id_contenue });
            }
            catch (Exception e)
            {
                TempData["error"] = "Erreur d'insertion";
                return RedirectToAction(nameof(Index), new { id_produit = id_produit, id_contenue });
            }
        }
        
    }

}
