using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Produits;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Produits
{
    public class FraisImportationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FraisImportationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FraisImportation
        [CheckAdminLevel5]
        public async Task<IActionResult> Index(int id_categorie)
        {
            var applicationDbContext = _context.FraisImportation
                .Include(f => f.CategorieProduit)
                .Where(q => q.IdCategorie == id_categorie)
                .OrderByDescending(q => q.DateDeb);
            ViewBag.id_categorie = id_categorie;
            ViewBag.categorie = await _context.CategorieProduit.FindAsync(id_categorie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FraisImportation/Details/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.FraisImportation == null)
            {
                return NotFound();
            }
            var fraisImportation = await _context.FraisImportation
                .Include(f => f.CategorieProduit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fraisImportation == null)
            {
                return NotFound();
            }

            return View(fraisImportation);
        }

        // GET: FraisImportation/Create
        [CheckAdminLevel5]
        public IActionResult Create(int id_categorie)
        {
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit.Where(q => q.Id== id_categorie), "Id", "Nom");
            ViewBag.id_categorie = id_categorie;
            return View();
        }

        // POST: FraisImportation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCategorie,MontantEuro,DateDeb,DateFin")] FraisImportation fraisImportation)
        {
            if (ModelState.IsValid)
            {
                await ProduitService.SaveFraisImportation(_context, fraisImportation);
                return RedirectToAction(nameof(Index), new { id_categorie = fraisImportation.IdCategorie });
            }
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit, "Id", "Nom", fraisImportation.IdCategorie);
            ViewBag.id_categorie = fraisImportation.IdCategorie;
            return View(fraisImportation);
        }

        // GET: FraisImportation/Edit/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Edit(long? id, int id_categorie)
        {
            if (id == null || _context.FraisImportation == null)
            {
                return NotFound();
            }

            var fraisImportation = await _context.FraisImportation.FindAsync(id);
            if (fraisImportation == null)
            {
                return NotFound();
            }
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit.Where(q => q.Id== id_categorie), "Id", "Nom", fraisImportation.IdCategorie);
            ViewBag.id_categorie = id_categorie;
            return View(fraisImportation);
        }

        // POST: FraisImportation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,IdCategorie,MontantEuro,DateDeb,DateFin")] FraisImportation fraisImportation)
        {
            if (id != fraisImportation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    fraisImportation.DateDeb = DateTimeToUTC.Make(fraisImportation.DateDeb);
                    if (fraisImportation.DateFin != null) fraisImportation.DateFin = DateTimeToUTC.Make((DateTime)fraisImportation.DateFin);
                    _context.Update(fraisImportation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FraisImportationExists(fraisImportation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id_categorie = fraisImportation.IdCategorie });
            }
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit, "Id", "Nom", fraisImportation.IdCategorie);
            ViewBag.id_categorie = fraisImportation.IdCategorie;
            return View(fraisImportation);
        }

        // GET: FraisImportation/Delete/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.FraisImportation == null)
            {
                return NotFound();
            }

            var fraisImportation = await _context.FraisImportation
                .Include(f => f.CategorieProduit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fraisImportation == null)
            {
                return NotFound();
            }

            return View(fraisImportation);
        }

        // POST: FraisImportation/Delete/5
        [CheckAdminLevel5]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.FraisImportation == null)
            {
                return Problem("Entity set 'ApplicationDbContext.FraisImportation'  is null.");
            }
            var fraisImportation = await _context.FraisImportation.FindAsync(id);
            if (fraisImportation != null)
            {
                _context.FraisImportation.Remove(fraisImportation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id_categorie = fraisImportation.IdCategorie});
        }

        private bool FraisImportationExists(long id)
        {
          return (_context.FraisImportation?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
