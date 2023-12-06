using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Data;
using originalstoremada.Models.Users;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Users.Scafold
{
    public class Admin_SController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Admin_SController(ApplicationDbContext context)
        {
            _context = context;
        }

        [CheckAdminLevel5]
        // GET: Admin_S
        public async Task<IActionResult> Index()
        {
              return _context.Admin != null ? 
                          View(await _context.Admin.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Admin'  is null.");
        }

        [CheckAdminLevel5]
        // GET: Admin_S/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Admin == null)
            {
                return NotFound();
            }

            var admin = await _context.Admin
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [CheckAdminLevel5]
        // GET: Admin_S/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin_S/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Mail,Nom,Prenom,Adresse,Niveau,Code")] Admin admin)
        {
            admin.Code = BCrypt.Net.BCrypt.HashPassword(admin.Code, BCrypt.Net.BCrypt.GenerateSalt());
            _context.Add(admin);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [CheckAdminLevel5]
        // GET: Admin_S/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Admin == null)
            {
                return NotFound();
            }

            var admin = await _context.Admin.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            return View(admin);
        }

        // POST: Admin_S/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminLevel5]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Mail,Nom,Prenom,Adresse,Niveau,Code")] Admin admin)
        {
            if (id != admin.Id)
            {
                return NotFound();
            }

            try
            {
                admin.Code = BCrypt.Net.BCrypt.HashPassword(admin.Code, BCrypt.Net.BCrypt.GenerateSalt());
                _context.Update(admin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(admin.Id))
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

        [CheckAdminLevel5]
        // GET: Admin_S/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Admin == null)
            {
                return NotFound();
            }

            var admin = await _context.Admin
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admin_S/Delete/5
        [CheckAdminLevel5]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Admin == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Admin'  is null.");
            }
            var admin = await _context.Admin.FindAsync(id);
            if (admin != null)
            {
                _context.Admin.Remove(admin);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
          return (_context.Admin?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
