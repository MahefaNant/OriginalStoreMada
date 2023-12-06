using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using originalstoremada.Data;
using originalstoremada.Models.Boutique;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Users;
using originalstoremada.Services.Boutiques;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.Boutiques
{
    public class BoutiqueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoutiqueController(ApplicationDbContext context)
        {
            _context = context;
        }

        [CheckAdminLevel5]
        public async Task<IActionResult> Affectation(int id_boutique)
        {
            Boutique? boutique = await _context.Boutique.FirstOrDefaultAsync(q => q.Id == id_boutique);
            ViewBag.boutique = boutique;
            List<Admin> admins = await _context.Admin.Where(q => q.Niveau != 5).ToListAsync();
            admins = await AdminService.AdminWithAffectation(_context, admins);
            ViewBag.employers = admins;
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View();
        }

        [CheckAdminLevel5]
        [HttpPost]
        public IActionResult Affectation(int id_boutique, List<int> id_admins)
        {
            try
            {
                foreach(var id in id_admins)
                {
                    AffectationEmployer affectation = new AffectationEmployer();
                    affectation.IdBoutique = id_boutique;
                    affectation.IdAdmin = id;
                    affectation.DateDeb = DateTime.UtcNow;
                    _context.Add(affectation);
                }
                
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }
            return RedirectToAction("Affectation" , new { id_boutique });
        }

        [CheckAdminLevel5]
        public async Task<IActionResult> AffectationRemove(int id_affectation)
        {
            var affect = _context.AffectationEmployer.FirstOrDefault(q => q.Id == id_affectation);
            affect.DateFin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction("Affectation", new { id_boutique = affect.IdBoutique });
        }

        /*--------------------------------------------------------------*/

        // GET: Boutique
        [CheckAdminLevel5]
        public async Task<IActionResult> Index()
        {
              return _context.Boutique != null ? 
                          View(await _context.Boutique.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Boutique'  is null.");
        }

        // GET: Boutique/Details/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Boutique == null)
            {
                return NotFound();
            }

            var boutique = await _context.Boutique
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boutique == null)
            {
                return NotFound();
            }

            return View(boutique);
        }

        // GET: Boutique/Create
        [CheckAdminLevel5]
        public async Task<IActionResult> Create()
        {
            try
            {
                var boutiques = await BoutiqueService.AllBoutique(_context);
                ViewBag.boutiques = boutiques;
                
                if (TempData.TryGetValue("error", out var error))
                {
                    ViewBag.error = error;
                }
                
                return View();
            }
            catch (Exception e)
            {
                TempData["error"] = "Une erreur est survenue";
                return RedirectToAction("Home", "Admin");
            }
            
        }

        // POST: Boutique/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Ville,Quartier,adresse,Description,Longitude,Latitude")] Boutique boutique)
        {
            try
            {
                if (boutique.adresse.IsNullOrEmpty() || boutique.Description.IsNullOrEmpty() ||
                    boutique.Nom.IsNullOrEmpty() || boutique.Quartier.IsNullOrEmpty()
                    || boutique.Ville.IsNullOrEmpty()) throw new Exception();
                _context.Add(boutique);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                TempData["error"] = "Erreur d'insertion";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Boutique/Edit/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Boutique == null)
            {
                return NotFound();
            }

            var boutique = await _context.Boutique.FindAsync(id);
            if (boutique == null)
            {
                return NotFound();
            }
            return View(boutique);
        }

        // POST: Boutique/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Ville,Quartier,adresse,Description,Longitude,Latitude")] Boutique boutique)
        {
            if (id != boutique.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boutique);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoutiqueExists(boutique.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(boutique);
        }

        // GET: Boutique/Delete/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Boutique == null)
            {
                return NotFound();
            }

            var boutique = await _context.Boutique
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boutique == null)
            {
                return NotFound();
            }

            return View(boutique);
        }

        // POST: Boutique/Delete/5
        [CheckAdminLevel5]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Boutique == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Boutique'  is null.");
            }
            var boutique = await _context.Boutique.FindAsync(id);
            if (boutique != null)
            {
                _context.Boutique.Remove(boutique);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoutiqueExists(int id)
        {
          return (_context.Boutique?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
