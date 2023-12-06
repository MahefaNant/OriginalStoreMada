using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Others;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.views;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Produits
{
    public class CategorieProduitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategorieProduitController(ApplicationDbContext context)
        {
            _context = context;
        }

        [CheckAdminAll]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<VCategorieProduit> categorieProduits = await _context.VCategorieProduit.ToListAsync();
                return View(categorieProduits);
            }
            catch (Exception e)
            {
                TempData["error"] = "Une erreur est survenue ! Contactez-nous!";
                return Ok();
            }
        }
        
        [CheckAdminAll]
        [HttpPost]
        public async Task<IActionResult> Create(string? Nom, long? id_produit, string? Code, double Frais, double Commission, int ver = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(Nom) || string.IsNullOrEmpty(Code)|| Frais<0 || Commission<0)
                {
                    TempData["error"] = "Information incomplete!";
                    if(ver == 1) return RedirectToAction("Create","Produit",new { id_produit });
                    return RedirectToAction("Index", "CategorieProduit");
                }
                CategorieProduit categorieProduit = new CategorieProduit() { Nom = Nom};
                categorieProduit.Code = Code;
                _context.Add(categorieProduit);
                await _context.SaveChangesAsync();

                var frais = new FraisImportation()
                {
                    IdCategorie = categorieProduit.Id, MontantEuro = Frais, DateDeb = DateTimeToUTC.Make(DateTime.Now)
                };
                var comms = new Commission()
                {
                    IdCategorie = categorieProduit.Id, MontantEuro = Commission,
                    DateDeb = DateTimeToUTC.Make(DateTime.Now)
                };
                _context.Add(frais);
                _context.Add(comms);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // ignored
            }
            
            if(ver == 1) return RedirectToAction("Create","Produit",new { id_produit });
            return RedirectToAction("Index", "CategorieProduit");
        }

        // POST: CategorieProduit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [CheckAdminAll]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Nom, string Code , double Frais, double Commission)
        {
            try
            {
                var categ = await _context.CategorieProduit.FindAsync(id);
                if (!categ.Nom.ToLower().Equals(Nom.ToLower()) || !categ.Code.ToLower().Equals(Nom.ToLower()))
                {
                    categ.Nom = Nom;
                    categ.Code = Code;
                    _context.Update(categ);
                }

                if (Frais >= 0 && Commission >= 0)
                {
                    var VC = await _context.VCategorieProduit
                        .FirstOrDefaultAsync(q => q.Id == categ.Id);
                    if (Math.Round(VC.MontantFraisImportation,2) != Math.Round(Frais,2))
                    {
                        var frais = new FraisImportation()
                        {
                            IdCategorie = categ.Id, MontantEuro = Frais, DateDeb = DateTimeToUTC.Make(DateTime.Now)
                        };
                        _context.Add(frais);
                    }

                    if (Math.Round(VC.MontantCommssion,2) != Math.Round(Commission,2))
                    {
                        var comms = new Commission()
                        {
                            IdCategorie = categ.Id, MontantEuro = Commission,
                            DateDeb = DateTimeToUTC.Make(DateTime.Now)
                        };
                        _context.Add(comms);
                    }
                   
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            return RedirectToAction(nameof(Index));
        }
        
        // POST: CategorieProduit/Delete/5
        [CheckAdminAll]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CategorieProduit == null)
            {
                return Problem("Entity set 'ApplicationDbContext.CategorieProduit'  is null.");
            }
            var categorieProduit = await _context.CategorieProduit.FindAsync(id);
            if (categorieProduit != null)
            {
                _context.CategorieProduit.Remove(categorieProduit);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategorieProduitExists(int id)
        {
          return (_context.CategorieProduit?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
