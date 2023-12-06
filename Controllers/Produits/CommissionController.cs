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
    public class CommissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Commission
        [CheckAdminLevel5]
        public async Task<IActionResult> Index(int id_categorie)
        {
            var applicationDbContext = _context.Commission
                .Include(c => c.CategorieProduit)
                .Where(q => q.IdCategorie == id_categorie)
                .OrderByDescending(q => q.DateDeb);
            ViewBag.id_categorie = id_categorie;
            ViewBag.categorie = await _context.CategorieProduit.FindAsync(id_categorie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Commission/Details/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.Commission == null)
            {
                return NotFound();
            }

            var commission = await _context.Commission
                .Include(c => c.CategorieProduit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commission == null)
            {
                return NotFound();
            }

            return View(commission);
        }

        // GET: Commission/Create
        [CheckAdminLevel5]
        public IActionResult Create(int id_categorie)
        {
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit.Where(q => q.Id== id_categorie), "Id", "Nom");
            return View();
        }

        // POST: Commission/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCategorie,MontantEuro,DateDeb,DateFin")] Commission commission)
        {
            if (ModelState.IsValid)
            {
                await ProduitService.SaveCommission(_context, commission);
                return RedirectToAction(nameof(Index), new { id_categorie = commission.IdCategorie });
            }
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit, "Id", "Nom", commission.IdCategorie);
            ViewBag.id_categorie = commission.IdCategorie;
            return View(commission);
        }

        // GET: Commission/Edit/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Edit(long? id, int id_categorie)
        {
            if (id == null || _context.Commission == null)
            {
                return NotFound();
            }

            var commission = await _context.Commission.FindAsync(id);
            if (commission == null)
            {
                return NotFound();
            }
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit.Where(q => q.Id== id_categorie), "Id", "Nom", commission.IdCategorie);
            ViewBag.id_categorie = id_categorie;
            return View(commission);
        }

        // POST: Commission/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,IdCategorie,MontantEuro,DateDeb,DateFin")] Commission commission)
        {
            if (id != commission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    commission.DateDeb = DateTimeToUTC.Make(commission.DateDeb);
                    if (commission.DateFin != null) commission.DateFin = DateTimeToUTC.Make((DateTime)commission.DateFin);
                    _context.Update(commission);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommissionExists(commission.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id_categorie = commission.IdCategorie });
            }
            ViewData["IdCategorie"] = new SelectList(_context.CategorieProduit, "Id", "Nom", commission.IdCategorie);
            ViewBag.id_categorie = commission.IdCategorie;
            return View(commission);
        }

        // GET: Commission/Delete/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Commission == null)
            {
                return NotFound();
            }

            var commission = await _context.Commission
                .Include(c => c.CategorieProduit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commission == null)
            {
                return NotFound();
            }

            return View(commission);
        }

        // POST: Commission/Delete/5
        [CheckAdminLevel5]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Commission == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Commission'  is null.");
            }
            var commission = await _context.Commission.FindAsync(id);
            if (commission != null)
            {
                _context.Commission.Remove(commission);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id_categorie = commission.IdCategorie });
        }

        private bool CommissionExists(long id)
        {
          return (_context.Commission?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
