using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutique;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.StatAdmin;
using originalstoremada.Models.Users;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Users;

namespace originalstoremada.Controllers.Users;

public class AdminController : Controller
{
    
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;    
    
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly string Key = KeyStorage.KeyAdmin;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    [CheckAdminAll]
    [HttpGet("Admin")]
    public async Task<IActionResult> Home(int? Annee)
    {
        try
        {
            Admin admin = JsonConvert.DeserializeObject<Admin>(Request.Cookies[Key]);

            admin = await _context.Admin.FindAsync(admin.Id);

            var beneficeReel = new VBeneficeReel();
            List<double> dataBeneficeReel = new List<double>();
            List<double> dataRecetteDevis = new List<double>();
            List<double> dataRecetteFacture = new List<double>();
            List<double> dataRecetteDivers = new List<double>();
            List<double> dataDiversDepense = new List<double>();

            List<string> labelMois = new List<string>();

            if (Annee == null)
            {
                DateTime D = DateTimeToUTC.Make(DateTime.Now);
                Annee = D.Year;
            }

            List<VBeneficeReelParAnneeMois> beneficeReelParmoisAns = new List<VBeneficeReelParAnneeMois>();
            VBeneficeReelParAnnee beneficeReelParAnnee = new VBeneficeReelParAnnee()
            {
                Annee = (int)Annee , DepenseTotal = 0, RecetteFacture = 0,RecetteDevis = 0,RecetteDivers = 0,RecetteTotal = 0,
                BeneficeReel = 0
            };

            if (AdminService.IsLevel_5(admin))
            {
                beneficeReel = await _context.VBeneficeReel.FirstAsync();
                
                beneficeReelParmoisAns =
                    await _context.VBeneficeReelParAnneeMois.Where(q => q.Annee == Annee)
                        .OrderBy(q => q.Mois)
                        .ToListAsync();

                beneficeReelParAnnee = await _context.VBeneficeReelParAnnee.FirstOrDefaultAsync(q => q.Annee == Annee);

                foreach (var b in beneficeReelParmoisAns)
                {
                    Console.WriteLine();
                    
                    labelMois.Add(UtilesFonctions.Mois()[b.Mois - 1]);
                    
                    dataBeneficeReel.Add(b.BeneficeReel);
                    dataRecetteDevis.Add(b.RecetteDevis);
                    dataRecetteFacture.Add(b.RecetteFacture);
                    dataRecetteDivers.Add(b.RecetteDivers);
                    dataDiversDepense.Add(b.DepenseTotal);
                }

                ViewBag.super = true;
            }

            ViewBag.beneficeReel = beneficeReel;
            ViewBag.beneficeReelParmoisAns = beneficeReelParmoisAns;
            ViewBag.beneficeReelParAnnee = beneficeReelParAnnee;

            ViewBag.Annee = Annee;

            ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            
            ViewBag.dataBeneficeReel = JsonConvert.SerializeObject(dataBeneficeReel);
            ViewBag.dataRecetteDevis = JsonConvert.SerializeObject(dataRecetteDevis);
            ViewBag.dataRecetteFacture = JsonConvert.SerializeObject(dataRecetteFacture);
            ViewBag.dataRecetteDivers = JsonConvert.SerializeObject(dataDiversDepense);
            ViewBag.dataDiversDepense = JsonConvert.SerializeObject(dataDiversDepense);

        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Login));
        }
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View();
    }


    public IActionResult SignUp()
    {
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(Admin admin, string RCode)
    {
        try
        {
            bool ver = await _context.Admin.AnyAsync(q => q.Mail == admin.Mail);
            if (ver)
            {
                TempData["error"] = "Mail existant !";
                return RedirectToAction(nameof(SignUp));
            }
            
            if (admin.Code != RCode)
            {
                TempData["error"] = "Mot de passe non identique! ";
                return RedirectToAction(nameof(SignUp));
            }
            
            admin.Code = BCrypt.Net.BCrypt.HashPassword(admin.Code, BCrypt.Net.BCrypt.GenerateSalt());
            admin.Niveau = 0;
            _context.Add(admin);
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

    // GET
    public IActionResult Login()
    {
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(string mail , string code, bool remember)
    {
        Admin? admin = await _context.Admin
            .FromSqlInterpolated($"SELECT * FROM admin WHERE mail ILIKE '%' || {mail} || '%' AND code = crypt({code} , code)")
            .FirstOrDefaultAsync();
        if (admin != null)
        {
            Boutique boutique = null;
            if (!AdminService.IsLevel_5(admin))
            {
                AffectationEmployer? A = await _context.AffectationEmployer
                        
                    .Where(q => q.IdAdmin == admin.Id && q.DateFin == null)
                    .OrderByDescending(q => q.DateDeb)
                    .Include(q => q.Boutique)
                    .FirstOrDefaultAsync();
                if (A == null)
                {
                    ViewBag.error = "Compte en attente de confirmation!";
                    return View();
                }

                boutique = A.Boutique;
            }
               
            string serializeClient = JsonConvert.SerializeObject(admin);
            if (remember)
            {
                if(boutique!=null) Response.Cookies.Append( "boutique" , JsonConvert.SerializeObject(boutique), CookieFunction.OptionDay(30));
                Response.Cookies.Append( Key , serializeClient, CookieFunction.OptionDay(30));
            }
            else
            {
                if(boutique!=null) Response.Cookies.Append( "boutique" , JsonConvert.SerializeObject(boutique), CookieFunction.OptionMinute(30));
                Response.Cookies.Append( Key , serializeClient, CookieFunction.OptionMinute(30));
            }

            return RedirectToAction("Home", "Admin");
        }
        
        TempData["error"] = "Mail ou mots de passe Invalide";
        return RedirectToAction(nameof(Login));
    }
    
    public IActionResult LogOut()
    {
        if (Request.Cookies[Key] != null) Response.Cookies.Delete(Key);
        if (Request.Cookies["boutique"] != null) Response.Cookies.Delete("boutique");
        return RedirectToAction("Login", "Admin");
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> HomeClientImage()
    {
        var imgs = await _context.HomeImage.OrderBy(q => q.Id).ToListAsync();
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
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View(imgs);
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> UpdateHomeClientImage(List<IFormFile> Image , int Id , string Title , string SubTitle)
    {
        try
        {
            var imgs = await _context.HomeImage.FindAsync(Id);
            List<string> IMG = new List<string>()
            {
                imgs.Image
            };
            Guid guid = Guid.NewGuid();

            if (Image.Any())
            {
                ImageService.RemoveImages(_webHostEnvironment,"images/homeClient",IMG,true);
                List<string> img = ImageService.UploadAndResizeImages(_webHostEnvironment,guid,Image,"images/homeClient" ,1920, 930, false);
                ImageService.UploadAndResizeImages(_webHostEnvironment,guid,Image,"images/homeClient" ,190, 100, true);
                imgs.Image = img[0];
            }

            
            imgs.Title = Title;
            imgs.SubTitle = SubTitle;
            _context.Update(imgs);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction(nameof(HomeClientImage));
    }
}