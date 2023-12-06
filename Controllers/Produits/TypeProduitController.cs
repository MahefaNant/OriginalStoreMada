using Microsoft.AspNetCore.Mvc;
using originalstoremada.Data;
using originalstoremada.Models.Produits;

namespace originalstoremada.Controllers.Produits;

public class TypeProduitController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public TypeProduitController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Create(string? Nom, long? id_produit)
    {
        if (string.IsNullOrEmpty(Nom))
        {
            return RedirectToAction("Create","Produit");
        }

        TypeProduit typeProduit = new TypeProduit() { Nom = Nom};
        _context.Add(typeProduit);
        _context.SaveChanges();
        return RedirectToAction("Create","Produit",new { id_produit }); 
    }
}