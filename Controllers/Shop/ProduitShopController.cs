using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Others;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.Others;
using originalstoremada.Models.Produits.views;
using originalstoremada.Models.Users;
using originalstoremada.Services.Boutiques;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Shop;

public class ProduitShopController : Controller
{

    private readonly ApplicationDbContext _context;

    private ProduitShopService _produitShopService;
    // GET
    public ProduitShopController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index(int? pagId , string? NomProd , int? IdCategorie , int[]? Type, string? Prix, bool PourEnfant, bool EnPromotion, bool isPag = false)
    {
        _produitShopService = new ProduitShopService(context: _context);

        if (!isPag) pagId = 1;
        var produits = await _produitShopService.Produits(HttpContext,12, pagId, NomProd, IdCategorie, Type, Prix, PourEnfant, EnPromotion);
        
        produits = await ProduitShopService.FillIsMyFavoris(_context, HttpContext, produits);
        
        ViewBag.paginations = _produitShopService.Pagination;
        ViewData[BoutiqueService.BoutiquesAllName] = await BoutiqueService.AllBoutique(_context);
        ViewBag.allCategories = await _context.CategorieProduit.ToListAsync();
        List<TypeProduit> allType = await _context.TypeProduit.ToListAsync();
        ViewBag.PricesCherch = ProduitShopService.PricesCherch();

        
        
        //  --------------------------Session -------------------------- //
        if (HttpContext.Session.GetInt32(Recherche.IdCategorieName) != null)
            ViewBag.IdCategorie = HttpContext.Session.GetInt32(Recherche.IdCategorieName);
        
        allType = ProduitShopService.CheckedTypeProduit(HttpContext, allType);
        ViewBag.allType = allType;
        
        if (HttpContext.Session.GetString(Recherche.PourEnfant) != null)
            ViewBag.PourEnfant = HttpContext.Session.GetString(Recherche.PourEnfant);
        
        if (HttpContext.Session.GetString(Recherche.EnPromotion) != null)
            ViewBag.EnPromotion = HttpContext.Session.GetString(Recherche.EnPromotion);
        
        if (HttpContext.Session.GetString(Recherche.Price) != null)
            ViewBag.Price = HttpContext.Session.GetString(Recherche.Price);
        // ---------------------------------------------------------------//
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View(model: produits);
    }

    public async Task<IActionResult> Details(long? id_produit, long? id_contenue)
    {
        try
        {
            var prods = await ProduitShopService.ProduitDetails(_context , id_produit);
            if (prods == null) throw new Exception("Une Erreur s'est produite");
         
            // var stock = await PreferenceProduitService.StockProduit(_context, id_produit);
            
            var stocks = _context.VStockPreference
                .Include(p => p.Boutique)
                .Include(q => q.ContenueProduit)
                .Where(q => q.IdProduit == id_produit);
            
            var contenues = await _context.ContenueProduit.Where(q => q.IdProduit == id_produit)
                .OrderByDescending(q => q.IsPrincipal).ToListAsync();
            ContenueProduit contenueSelected;
            List<VStockGlobalParPreference> preferences = new List<VStockGlobalParPreference>();
            

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
                preferences = await _context.VStockGlobalParPreference
                    .Where(q => q.IdProduit == id_produit && q.IdContenue == contenueSelected.Id)
                    .OrderBy(q => q.Taille).ToListAsync();

                contenues = contenues.OrderBy(q => q.Id == contenueSelected.Id ? 0 : 1).ToList();
            }

            Client client = null;
            bool isFavoris = false;
            if (Request.Cookies[KeyStorage.KeyClient] != null)
            {
                client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
                var favoris = await _context.FavorisProduit.FirstOrDefaultAsync(q => q.IdClient == client.Id && q.IdProduit == id_produit);

                isFavoris = favoris != null;
            }
            ViewBag.isFavoris = isFavoris;
            ViewBag.client = client;
            
            ViewBag.id_produit = id_produit;
            ViewBag.contenues = contenues;
            ViewBag.stocks = await stocks.ToListAsync();
            ViewBag.contenueSelected = contenueSelected;
            ViewBag.preferences = preferences;
            // ViewBag.prefProd = PreferenceProduitService.PrefProds(stock);
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }

            return View(prods);
        }
        catch (Exception e)
        {
            TempData["error"] = "Une erreur s'est produite";
            return RedirectToAction(nameof(Index));
        }
        
    }

    public async Task<IActionResult> AddToCart(long id_produit, long id_pref, int quantiter)
    {
        try
        {
            if (quantiter <= 0) throw new Exception("Quantiter invalide!");
            await ProduitShopService.Cart(_context, HttpContext, id_produit, id_pref, quantiter);
            return RedirectToAction(nameof(Carts));
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Details) , new { id_produit });
        }
    }

    public async Task<IActionResult> Carts()
    {
        List<Cart> carts = new List<Cart>();
        
        try
        {
            carts = await ProduitShopService.GetAllCart(_context, HttpContext);
            if (HttpContext.Session.GetString("coordonnerChooseBoutique") != null)
            {
                try
                {
                    Coordonner coordonner = JsonConvert.DeserializeObject<Coordonner>(HttpContext.Session.GetString("coordonnerChooseBoutique"));
                    coordonner.Boutique = await _context.Boutique.FirstOrDefaultAsync(q => q.Id == coordonner.IdBoutique);
                    ViewBag.coordonner = coordonner;
                    ViewBag.infoPayement = PayementShopService.InfoPayementShop(coordonner, carts);
                }
                catch (Exception e)
                {
                    if(HttpContext.Session.GetString("coordonnerChooseBoutique")!=null)
                        HttpContext.Session.Remove("coordonnerChooseBoutique");
                }
            }
        }
        catch (Exception e)
        {
            if (Request.Cookies[KeyStorage.Carts] != null)
            {
                Response.Cookies.Delete(KeyStorage.Carts);
            }
        }
        ViewBag.summCarts = ProduitShopService.SommeCart(carts);
        var PayementsuccessMessage = TempData["Payementsuccess"] as string;
        if (!string.IsNullOrEmpty(PayementsuccessMessage))
        {
            ViewBag.Payementsuccess = PayementsuccessMessage;
            try
            {
                Client c = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
                var client = await _context.Client.FirstOrDefaultAsync(q => q.Id == c.Id);
                ViewBag.client = client;
            }
            catch (Exception e)
            {
                // ignored
            }
        }
        
        var typePayements = await _context.TypePayement.OrderByDescending(q => q.Nom).ToListAsync();
        ViewBag.typePayements = typePayements;
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        return View(carts);
    }

    public IActionResult RemoveCartElement(int id_pref)
    {
        List<Cart> carts = JsonConvert.DeserializeObject<List<Cart>>(Request.Cookies[KeyStorage.Carts]);
        foreach (var q in carts)
        {
            if (q.IdPref == id_pref)
            {
                carts.Remove(q);
                break;
            }
        }
        Response.Cookies.Delete(KeyStorage.Carts);
        if (carts.Count > 0) Response.Cookies.Append(KeyStorage.Carts,  JsonConvert.SerializeObject(carts), CookieFunction.OptionDay(7));
        return RedirectToAction(nameof(Carts));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCart(long[] idPrefs,int[] quants)
    {
        try
        {
            await ProduitShopService.UpdateCarts(_context,HttpContext,idPrefs , quants);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(Carts));
    }

    [CheckClient()]
    public async Task<IActionResult> ChooseBoutique(bool InBoutique = false)
    {
        try
        {
            var boutiques = await BoutiqueService.AllBoutique(_context);
            
            ViewBag.InBoutique = InBoutique;
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            return View(boutiques);
        }
        catch (Exception e)
        {
            TempData["error"] = "Accés refusé!";
            return RedirectToAction(nameof(Carts));
        }
    }

    [HttpPost]
    [CheckClient()]
    public async Task<IActionResult >ChooseBoutique(string? IdBoutique, Coordonner? coordonner)
    {
        // Idboutique[3] => Id
        Boutique? bout;
        int? id_b;
        try
        {
            bout = await PayementShopService.ChooseBoutique(_context, coordonner, IdBoutique);
            id_b = bout.Id;
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
           return RedirectToAction(nameof(ChooseBoutique));
        }

        coordonner.IdBoutique = (int)id_b;
        HttpContext.Session.SetString("coordonnerChooseBoutique", JsonConvert.SerializeObject(coordonner));
        return RedirectToAction(nameof(Carts));
    }

    [HttpPost]
    [CheckClient("ProduitShop", "Carts")]
    public async Task<IActionResult> Payement(int id_typePayement)
    {
        // await Task.Delay(500);
        try
        {
            await PayementShopService.Payement(_context, HttpContext, id_typePayement);
            TempData["Payementsuccess"] = "OK";
            return RedirectToAction("Facture", "MyShop");
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Carts));
        }
    }
    
    [HttpPost]
    public async Task<JsonResult> SetFavoris(int IdProduit)
    {
        int etatFavoris = await ProduitShopService.SetFavoris(_context, HttpContext, IdProduit);

        return Json(new { etatFavoris });
    }

    [HttpPost]
    public async Task<IActionResult> SetFavorisRhg(int IdProduit)
    {
        try
        {
            await ProduitShopService.SetFavoris(_context, HttpContext, IdProduit);
        }
        catch (Exception e)
        {
           TempData["message"] = e.Message;
        }

        return RedirectToAction(nameof(Details), new { id_produit = IdProduit });
    }

}