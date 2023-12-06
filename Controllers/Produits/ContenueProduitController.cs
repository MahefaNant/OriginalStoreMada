using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Produits;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Produits
{
    public class ContenueProduitController : Controller
    {
        private readonly ApplicationDbContext _context;
        public readonly IWebHostEnvironment webHostEnvironment;
        

        public ContenueProduitController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        [CheckAdminAll]
        [HttpPost]
        public async Task<IActionResult> ToPrincipal(long id_produit, int id_contenue)
        {
            try
            {
                var contents = await _context.ContenueProduit.Where(q => q.IdProduit == id_produit).ToListAsync();
                foreach (var c in contents)
                {
                    if (c.Id == id_contenue)
                    {
                        c.IsPrincipal = true;
                        _context.Update(c);
                    }
                    else if (c.IsPrincipal)
                    {
                        c.IsPrincipal = false;
                        _context.Update(c);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index","PreferenceProduit", new { id_produit, id_contenue });
            }
            catch (Exception e)
            {
                TempData["error"] = "Une erreur est survenue! Contactez-nous";
                return RedirectToAction("Index","PreferenceProduit", new { id_produit, id_contenue });
            }
        }

        [CheckAdminAll]
        [HttpPost]
        public async Task<IActionResult> Update(long id_produit, int id_contenue, List<IFormFile> Images, string couleur)
        {
            try
            {
                if (couleur.IsNullOrEmpty() && !Images.Any()) throw new Exception("Aucun modification!");

                var cont = await _context.ContenueProduit.FirstOrDefaultAsync(q => q.Id == id_contenue);
                if(cont == null) throw new Exception("Une Erreur Est survenue!");

                if (!couleur.ToLower().Equals(cont.Couleur.ToLower()))
                {
                    cont.Couleur = couleur;
                    _context.Update(cont);
                }

                if (Images.Any())
                {
                    Guid guid = Guid.NewGuid();
                    ImageService.RemoveImages(webHostEnvironment,"images/produits" ,new List<string>() { cont.Image }, true);
                    List<string> savedFilesNames = ImageService.UploadAndResizeImages(webHostEnvironment,guid,Images,"images/produits" ,1200, 1486, false);
                    ImageService.UploadAndResizeImages(webHostEnvironment,guid, Images,"images/produits" ,120, 120, true);
                    cont.Image = savedFilesNames[0];
                    _context.Update(cont);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index","PreferenceProduit", new { id_produit , id_contenue });
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
                return RedirectToAction("Index","PreferenceProduit", new { id_produit , id_contenue });
            }
        }
        
        /*---------------------------------------------------*/

    }
}
