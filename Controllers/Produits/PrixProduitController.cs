using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Data;
using originalstoremada.Models.Produits;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Produits
{
    public class PrixProduitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrixProduitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PrixProduit
        [CheckAdminLevel5]
        public async Task<IActionResult> Index(long id_produit)
        {
            var applicationDbContext = _context.PrixProduit
                .Where( q => q.IdProduit == id_produit)
                .OrderByDescending(q => q.DateDeb)
                .Include(p => p.Produit);
            ViewBag.id_produit = id_produit;
            ViewBag.produit = await _context.Produit.FindAsync(id_produit);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PrixProduit/Details/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.PrixProduit == null)
            {
                return NotFound();
            }

            var prixProduit = await _context.PrixProduit
                .Include(p => p.Produit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prixProduit == null)
            {
                return NotFound();
            }

            return View(prixProduit);
        }

        // GET: PrixProduit/Create
        [CheckAdminLevel5]
        public IActionResult Create(long id_produit)
        {
            ViewData["IdProduit"] = new SelectList(_context.Produit.Where( q => q.Id == id_produit), "Id", "Nom");
            ViewBag.id_produit = id_produit;
            return View();
        }

        // POST: PrixProduit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdProduit,PrixAchat,PrixVente,DateDeb,DateFin")] PrixProduit prixProduit)
        {
            if (ModelState.IsValid)
            {
                await ProduitService.SavePrixProduit(_context, prixProduit);
                return RedirectToAction(nameof(Index), new { id_produit = prixProduit.IdProduit});
            }
            ViewData["IdProduit"] = new SelectList(_context.Produit.Where( q => q.Id == prixProduit.IdProduit), "Id", "Nom", prixProduit.IdProduit);
            ViewBag.id_produit = prixProduit.IdProduit;
            return View(prixProduit);
        }

        // GET: PrixProduit/Edit/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Edit(long? id, long id_produit)
        {
            if (id == null || _context.PrixProduit == null)
            {
                return NotFound();
            }

            var prixProduit = await _context.PrixProduit.FindAsync(id);
            if (prixProduit == null)
            {
                return NotFound();
            }
            ViewData["IdProduit"] = new SelectList(_context.Produit.Where( q => q.Id == id_produit), "Id", "Nom", prixProduit.IdProduit);
            ViewBag.id_produit = id_produit;
            return View(prixProduit);
        }

        // POST: PrixProduit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,IdProduit,PrixAchat,PrixVente,DateDeb,DateFin")] PrixProduit prixProduit)
        {
            if (id != prixProduit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prixProduit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrixProduitExists(prixProduit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id_produit = prixProduit.IdProduit});
            }
            ViewData["IdProduit"] = new SelectList(_context.Produit.Where( q => q.Id == prixProduit.IdProduit), "Id", "Nom", prixProduit.IdProduit);
            ViewBag.id_produit = prixProduit.IdProduit;
            return View(prixProduit);
        }

        // GET: PrixProduit/Delete/5
        [CheckAdminLevel5]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.PrixProduit == null)
            {
                return NotFound();
            }

            var prixProduit = await _context.PrixProduit
                .Include(p => p.Produit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prixProduit == null)
            {
                return NotFound();
            }

            return View(prixProduit);
        }

        // POST: PrixProduit/Delete/5
        [CheckAdminLevel5]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.PrixProduit == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PrixProduit'  is null.");
            }
            var prixProduit = await _context.PrixProduit.FindAsync(id);
            if (prixProduit != null)
            {
                _context.PrixProduit.Remove(prixProduit);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id_produit = prixProduit.IdProduit});
        }

        private bool PrixProduitExists(long id)
        {
          return (_context.PrixProduit?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
