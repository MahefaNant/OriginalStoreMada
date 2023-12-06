using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Produits.views;
using originalstoremada.Models.Users;
using originalstoremada.Services.Boutiques;
using originalstoremada.Services.Produits;
using originalstoremada.Services.Users;


namespace originalstoremada.Controllers.Users;

public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly string Key = KeyStorage.KeyClient;

    public ClientController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET
    // [TypeFilter(typeof(CheckBoutique))]
    public async Task<IActionResult> Home(int etat = 0)
    {
        if (etat > 1) etat = 0;
        try
        {
            var imgs = await _context.HomeImage.OrderBy(q => q.Id).ToListAsync();
            Console.WriteLine(JsonConvert.SerializeObject(imgs));
            if (!imgs.Any())
            {
                for (int i = 0; i < 3; i++)
                {
                    HomeImage homeImage = new HomeImage()
                    {
                        Image = "vide.jpg" , Title = "Titre" , SubTitle = "sous Titre"
                    };
                    _context.Add(homeImage);
                }
                await _context.SaveChangesAsync();
                imgs = await _context.HomeImage.OrderBy(q => q.Id).ToListAsync();
            }

            IQueryable<VStockProduitGlobal> PRODS = _context.VStockProduitGlobal;

            if(etat == 0)
                PRODS = PRODS.OrderByDescending(q => q.NombreSeller);
            if(etat == 1)
                PRODS = PRODS.Where(q => q.NombreFavoris>0).OrderByDescending(q => q.NombreFavoris);

            var produits = await PRODS.Take(4).ToListAsync();

            produits = await ProduitShopService.FillIsMyFavoris(_context, HttpContext, produits);

            ViewBag.produits = produits;
            ViewBag.images = imgs;
        }
        catch (Exception e)
        {
            // ignored
        }

        ViewBag.etat = etat;
        return View();
    }

    public async Task<IActionResult> SignUp()
    {
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> SignUp([Bind("Id,Nom,Prenom,Mail,Adresse,Code")] Client client, string RCode)
    {
        try
        {
            if (await ClientService.IsMailExiste(_context, client.Mail))
            {
                TempData["error"] = "Mail existant !";
                return RedirectToAction(nameof(SignUp));
            }

            if (client.Code != RCode)
            {
                TempData["error"] = "Mot de passe non identique! ";
                return RedirectToAction(nameof(SignUp));
            }
            client.Code = BCrypt.Net.BCrypt.HashPassword(client.Code, BCrypt.Net.BCrypt.GenerateSalt());
            _context.Add(client);
            await _context.SaveChangesAsync();
            
            TempData["message"] = "Compte ajouter avec succés!";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Login));
        }
        
    }
    
    public string RedirectController = "asdsa";
    
    // [HttpGet("Client/Login/{RedirectController?}/{RedirectAction?}")]
    public async Task<IActionResult> Login(string? RedirectController,string? RedirectAction)
    {
        ViewData["RedirectController"] = RedirectController;
        ViewData["RedirectAction"] = RedirectAction;
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        if (TempData.TryGetValue("message", out var message))
        {
            ViewBag.message = message;
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string mail , string code, bool remember, string RedirectController = "Client", string RedirectAction = "Home")
    {
        // Client? client = await ClientService.Login(_context, mail);
        Console.WriteLine(mail);
        Console.WriteLine(code);
        Client? client = await _context.Client
            .FromSqlInterpolated($"SELECT * FROM client WHERE mail ILIKE '%' || {mail} || '%' AND code = crypt({code} , code)")
            .FirstOrDefaultAsync();


        if (client != null)
        {
            string serializeClient = JsonConvert.SerializeObject(client);
            if (remember)
            {
                Response.Cookies.Append( Key , serializeClient, CookieFunction.OptionDay(30));
            }
            else
            {
                Response.Cookies.Append( Key , serializeClient, CookieFunction.OptionMinute(30));
            }
            return RedirectToAction(RedirectAction, RedirectController);
        }
        TempData["error"] = "Mail ou mots de passe Invalide";
        return RedirectToAction("Login", "Client", new { RedirectController, RedirectAction });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeBoutique(int? id_boutique)
    {
        if (id_boutique == null) return RedirectToAction(nameof(Home));
        Boutique boutique = await _context.Boutique.FindAsync(id_boutique);
        BoutiqueService.ChangeBoutiqueCookies(boutique, HttpContext);
        return RedirectToAction(nameof(Home));
    }
    
    public IActionResult LogOut()
    {
        // CheckBoutique.Ckeck(_context ,HttpContext);
        if (Request.Cookies[Key] != null) Response.Cookies.Delete(Key);
        return RedirectToAction("Login", "Client");
    }
    
    public async Task<IActionResult> GetIconStatus()
    {
        bool isIcon2 = true; // Vous pouvez déterminer l'état de l'icône ici
        return Json(new { iconChange = isIcon2 });
    }
    
    
}