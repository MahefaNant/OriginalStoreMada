
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Users;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.Produits
{
    public class EntreeProduitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntreeProduitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EntreeProduit
        [CheckAdminAll]
        public async Task<IActionResult> Index(long? id_produit)
        {
            var applicationDbContext = _context.EntreeProduit
                .Include(e => e.Boutique).Include(e => e.PreferenceProduit).Include(e => e.Produit)
                .Where(q => q.IdProduit == id_produit)
                .OrderByDescending(q => q.Date);
            ViewBag.id_produit = id_produit;
            ViewBag.produit = await _context.Produit.FindAsync(id_produit);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: EntreeProduit/Details/5
        [CheckAdminAll]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || _context.EntreeProduit == null)
            {
                return NotFound();
            }

            var entreeProduit = await _context.EntreeProduit
                .Include(e => e.Boutique)
                .Include(e => e.PreferenceProduit)
                .Include(e => e.Produit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entreeProduit == null)
            {
                return NotFound();
            }

            return View(entreeProduit);
        }

        // GET: EntreeProduit/Create
        [CheckAdminAll]
        public async Task<IActionResult> Create(long? id_produit, long? id_preference)
        {
            Admin admin = AdminService.GetByCookies(Request.Cookies[KeyStorage.KeyAdmin]);
            ViewBag.admin = admin;
            if (!AdminService.IsLevel_5(admin))
            {
                Boutique boutique = JsonConvert.DeserializeObject<Boutique>(Request.Cookies[KeyStorage.KeyBoutique]);
                ViewBag.boutique = await _context.Boutique.Where(q => q.Id == boutique.Id).FirstOrDefaultAsync();
                ViewBag.preference = await _context.PreferenceProduit.Where(q => q.Id==id_preference).FirstOrDefaultAsync();
                ViewBag.Vproduit = await _context.VImagePrincipalPrixProduit
                    .Include(q => q.CategorieProduit)
                    .Where(q => q.Id == id_produit).FirstOrDefaultAsync();   
            }
            else
            {
                if(id_preference!=null) ViewBag.preference = await _context.PreferenceProduit.Where(q => q.Id==id_preference).FirstOrDefaultAsync();
                ViewBag.boutiques = _context.Boutique.Select(q => new SelectListItem
                {
                    Text = $"{q.Quartier}/{q.Ville}({q.adresse})",
                    Value = q.Id.ToString()
                }).ToList();
                ViewBag.Vproduit = await _context.VImagePrincipalPrixProduit
                    .Include(q => q.CategorieProduit)
                    .Where(q => q.Id == id_produit).FirstOrDefaultAsync();
            }
            return View();
        }

        // POST: EntreeProduit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminAll]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdBoutique,IdProduit,IdPreferenceProduit,Quantiter,Date")] EntreeProduit entreeProduit, int? id_boutique, long? IdContenue, string? taille)
        {
            Admin admin = AdminService.GetByCookies(Request.Cookies[KeyStorage.KeyAdmin]);
            Boutique boutique = null;
            if(Request.Cookies[KeyStorage.KeyBoutique]!=null)
                boutique = JsonConvert.DeserializeObject<Boutique>(Request.Cookies[KeyStorage.KeyBoutique]);
            entreeProduit.Date = DateTimeToUTC.Make(entreeProduit.Date);
            await EntreeProduitService.AjoutProduitDynamique(_context, entreeProduit, admin, boutique , IdContenue, taille);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create) , new { id_produit = entreeProduit.IdProduit, id_preference = entreeProduit.IdPreferenceProduit });
        }

        // GET: EntreeProduit/Edit/5
        [CheckAdminAll]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.EntreeProduit == null)
            {
                return NotFound();
            }

            var entreeProduit = await _context.EntreeProduit.FindAsync(id);
            if (entreeProduit == null)
            {
                return NotFound();
            }
            ViewData["IdBoutique"] = new SelectList(_context.Boutique, "Id", "Id", entreeProduit.IdBoutique);
            ViewData["IdPreferenceProduit"] = new SelectList(_context.PreferenceProduit, "Id", "Id", entreeProduit.IdPreferenceProduit);
            ViewData["IdProduit"] = new SelectList(_context.Produit, "Id", "Id", entreeProduit.IdProduit);
            return View(entreeProduit);
        }

        // POST: EntreeProduit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminAll]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,IdBoutique,IdProduit,IdPreferenceProduit,Quantiter,Date")] EntreeProduit entreeProduit)
        {
            if (id != entreeProduit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entreeProduit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntreeProduitExists(entreeProduit.Id))
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
            ViewData["IdBoutique"] = new SelectList(_context.Boutique, "Id", "Id", entreeProduit.IdBoutique);
            ViewData["IdPreferenceProduit"] = new SelectList(_context.PreferenceProduit, "Id", "Id", entreeProduit.IdPreferenceProduit);
            ViewData["IdProduit"] = new SelectList(_context.Produit, "Id", "Id", entreeProduit.IdProduit);
            return View(entreeProduit);
        }

        // GET: EntreeProduit/Delete/5
        [CheckAdminAll]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.EntreeProduit == null)
            {
                return NotFound();
            }

            var entreeProduit = await _context.EntreeProduit
                .Include(e => e.Boutique)
                .Include(e => e.PreferenceProduit)
                .Include(e => e.Produit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entreeProduit == null)
            {
                return NotFound();
            }

            return View(entreeProduit);
        }

        // POST: EntreeProduit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.EntreeProduit == null)
            {
                return Problem("Entity set 'ApplicationDbContext.EntreeProduit'  is null.");
            }
            var entreeProduit = await _context.EntreeProduit.FindAsync(id);
            if (entreeProduit != null)
            {
                _context.EntreeProduit.Remove(entreeProduit);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntreeProduitExists(long id)
        {
          return (_context.EntreeProduit?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
