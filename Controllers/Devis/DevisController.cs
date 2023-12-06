using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Devis;
using originalstoremada.Models.Devis.Views;
using originalstoremada.Models.Others;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.Users;
using originalstoremada.Services;
using originalstoremada.Services.Boutiques;
using originalstoremada.Services.Devis;
using originalstoremada.Services.Mail;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Devis;

public class DevisController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly SmtpConfig _smtpConfig;

    public DevisController(ApplicationDbContext context, SmtpConfig smtpConfig)
    {
        _context = context;
        _smtpConfig = smtpConfig;
    }

    
    // GET
    [CheckClient()]
    public async Task<IActionResult> Index(int? pagId, bool IsPag = false,int type =0)
    {
        try
        {
            List<VDevisInfo> res = null;
            if (type != 1)
            {
                if (!IsPag)
                {
                    HttpContext.Session.SetInt32("typeRechDev", type);
                }
                else
                {
                    type = (int)HttpContext.Session.GetInt32("typeRechDev");
                }
                IQueryable<VDevisInfo> devis = DevisService.MyDevis(_context, HttpContext, type, false);
                if (!IsPag) pagId = 1;
                Pagination<VDevisInfo> pagination = new Pagination<VDevisInfo>(10, pagId, devis);
                devis = pagination.Paginate();
            
                res = await devis.ToListAsync();
                if(res.Count() >0) DevisService.SetAllEtatDevis(res);
                ViewBag.paginations = pagination;
            }
            
            ViewBag.devis = await DevisService.MelangeMyDevis(_context, HttpContext , res, type);
            ViewBag.type = type;
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction("Home", "Client");
        }
        ViewData["categories"] = new SelectList (_context.CategorieProduit, "Id", "Nom");
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        return View();
    }

    public IActionResult CreateSimulateCommande(CommandeDevis commandeDevis)
    {
        DevisService.CreateSimulateCommandeDevis(HttpContext, commandeDevis);
        return RedirectToAction(nameof(Create),new {id_devis = commandeDevis.IdDevis});
    }

    public IActionResult DeleteSimulateCommande(int? id, int? IDevis, bool isAll)
    {
        DevisService.DeleteSimulateCommandeDevis(HttpContext, id, IDevis, isAll);
        return RedirectToAction(nameof(Create), new {id_devis = IDevis});
    }

    public IActionResult UpdateSimulateCommandeDevis(CommandeDevis commandeDevis)
    {
        DevisService.UpdateSimulateCommandeDevis(HttpContext, commandeDevis);
        return RedirectToAction(nameof(Create), new {id_devis = commandeDevis.IdDevis});
    }
    
    public async Task<IActionResult> Create(long? id_devis)
    {
        ViewData["categories"] = new SelectList (_context.CategorieProduit, "Id", "Nom");
        List<CommandeDevis> commandeDevis = new List<CommandeDevis>();
        try
        {
            commandeDevis = DevisService.DevisSimulateByIdDevis(HttpContext, id_devis);
            if(commandeDevis.Count<=0) return RedirectToAction(nameof(Index));
            ViewBag.devis = await DevisService.GetDevisBySimulate(_context, HttpContext, id_devis);
            ViewBag.IdDevis = id_devis;
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index));
        }
        ViewBag.commandeDevis = commandeDevis;
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        return View();
    }

    public async Task<IActionResult> Details(long? id_devis)
    {
        List<VCommanceDevis> commandeDevis = new List<VCommanceDevis>();
        try
        {
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
            var devis =await _context.VDevisInfos.FirstOrDefaultAsync(q => q.Id ==id_devis && q.IdClient == client.Id);
            DevisService.SetEtatDevis(devis);
            DevisService.SetEtatPrixDevis(devis);
            if (devis == null) throw new Exception("Error");
            commandeDevis = await _context.VCommanceDevis
                .Where(q => q.IdDevis == id_devis).ToListAsync();
            ViewBag.devis = devis;
            ViewBag.adresseLivraison = await _context.AdresseLivraisonDevis.FirstOrDefaultAsync(q => q.IdDevis == id_devis);
            ViewBag.suivisDevis = DevisService.SuivisDevisEtat(devis);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index));
        }
        
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        return View(commandeDevis);
    }
    
    [CheckClient]
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Id,IdDevis,IdCategorie,ProduitName,PrixEuro,PrixAriary,Couleur,Taille,Nombre,ReferenceSite,image")] CommandeDevis commandeDevis)
    {
        try
        {
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
            await DevisService.CreateCommandeDevis(_context, commandeDevis , client);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        
        return RedirectToAction(nameof(Create), new { id_devis = commandeDevis.IdDevis });
    }
    
    [CheckClient("Devis", "Index")]
    public async Task<IActionResult> DemanderDevis(int id_devis)
    {
        try
        {
            DevisService.DemanderDevis(_context,HttpContext,id_devis);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        return RedirectToAction("Create", new { id_devis });
    }
    
    /*-----------------PAYEMENT--------------------*/

    [CheckClient("Devis", "Index")]
    public async Task<IActionResult> Payement(long? id_devis)
    {
        List<VCommanceDevis> commandeDevis = new List<VCommanceDevis>();
        try
        {
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
            var devis =await _context.VDevisInfos.FirstOrDefaultAsync(q => q.Id ==id_devis && q.IdClient == client.Id);
            DevisService.SetEtatDevis(devis);
            DevisService.SetEtatPrixDevis(devis);
            if (devis == null) throw new Exception("Error");
            if (devis.DateDelete != null) throw new Exception("Invalide Devis");
            if(devis.EtatActuel.Etat == DevisService.Envoyer) throw new Exception("En attente de validation");
            commandeDevis = await _context.VCommanceDevis
                .Where(q => q.IdDevis == id_devis).ToListAsync();
            
            VPayementDevisEtat vPayementDevisEtat = await _context.VPayementDevisEtat
                .Include(q => q.Client).FirstOrDefaultAsync(q => q.IdDevis == id_devis);
            var payementsDevis = await _context.PayementDevis.Where(q => q.IdDevis == id_devis).ToListAsync();
            ViewBag.payements = payementsDevis;
            ViewBag.payementInfo = vPayementDevisEtat;
            Tranche[] tranches = DevisService.MontantPayementDiviser(devis, payementsDevis, true);
            ViewBag.tranches = tranches;
            ViewBag.nextRestes = DevisService.GetNextAndRestePayement(tranches);
            ViewBag.devis = devis;
            ViewBag.client = client;
            ViewBag.suivisDevis = DevisService.SuivisDevisEtat(devis);
            var typePayements = await _context.TypePayement.OrderByDescending(q => q.Nom).ToListAsync();
            ViewBag.typePayements = new SelectList(typePayements, "Id", "Nom");

            ViewBag.payementNonConfirmerForm = DevisService.PayementNonConfirmerForm(devis, payementsDevis, typePayements);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Details), new { id_devis });
        }

        var PayementsuccessMessage = TempData["Payementsuccess"] as string;
        if (!string.IsNullOrEmpty(PayementsuccessMessage))
        {
            ViewBag.Payementsuccess = PayementsuccessMessage;
        }
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }
        
        return View(commandeDevis);
    }

    [CheckClient]
    [HttpPost]
    public async Task<IActionResult> Payement(long id_devis, int indice, int typePayement)
    {
        // await Task.Delay(500);
        try
        {
            await DevisService.Payement(_context, HttpContext, _smtpConfig, id_devis, indice, typePayement);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Payement) , new { id_devis });
        }
        TempData["Payementsuccess"] = "OK";
        return RedirectToAction(nameof(Payement) , new { id_devis });
    }

    [CheckClient()]
    public async Task<IActionResult> MesPayement(int? pagId, bool isPagination = false, int etat = 0)
    {
        try
        {
            if (etat > 2) etat = 0;
            if (!isPagination) pagId = 1;
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
            
            IQueryable<VPayementDevisSum> queryPayd = _context.VPayementDevisSum
                .Include(q => q.Client)
                .Include(q => q.TypePayement)
                .OrderByDescending(q => q.Date)
                .Where(q => q.IdClient == client.Id);
            
            if (etat == 1) queryPayd = queryPayd.Where(q => q.DatePayement != null);
            else if (etat == 2) queryPayd = queryPayd.Where(q => q.DatePayement == null);
            
            Pagination<VPayementDevisSum> paginations = new Pagination<VPayementDevisSum>(10, pagId, queryPayd);
            paginations.Paginate();
            
            var model = await queryPayd.ToListAsync();
            
            ViewBag.isPagination = isPagination;
            ViewBag.pagId = pagId;
            ViewBag.etat = etat;


            ViewBag.paginations = paginations;
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(model);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Index));
        }
    }
    
    [CheckClient()]
    public async Task<IActionResult> OptionImportation(long id_devis)
    {
        try
        {
            var volCommande = await _context.VvolDevisLivraison
                .Include(q => q.CommandeDevis)
                .Include(q => q.Vol)
                .Where(q => q.IdDevis == id_devis)
                .OrderBy(q => q.Vol.DateDepart).ToListAsync();

            var commsInLivr = volCommande
                .Where(q => q.Vol.DateArriver != null).ToList();
            
           

            if (commsInLivr.Any())
            {
                var adresses = new List<VvolDevisLivraison>();
                ViewBag.AdresseExist = true;

                adresses = commsInLivr
                    .Where(q => q.InAdresse)
                    .GroupBy(q => new { q.Ville, q.Quartier, q.Longitude, q.Latitude, q.InAdresse, q.FraisLivraison })
                    .Select(g => new VvolDevisLivraison
                    {
                        Ville = g.Key.Ville,
                        Quartier = g.Key.Quartier,
                        Longitude = g.Key.Longitude,
                        Latitude = g.Key.Latitude,
                        InAdresse = g.Key.InAdresse,
                        FraisLivraison = g.Key.FraisLivraison
                    })
                    .AsEnumerable()
                    .Select(q => new VvolDevisLivraison
                    {
                        Ville = q.Ville,
                        Quartier = q.Quartier,
                        Longitude = q.Longitude,
                        Latitude = q.Latitude,
                        InAdresse = q.InAdresse,
                        FraisLivraison = q.FraisLivraison
                    })
                    .ToList();
                
                ViewBag.adresses = adresses;
            }

            ViewBag.id_devis = id_devis;
            ViewBag.commsInLivr = commsInLivr;
            
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(volCommande);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Details) , new { id_devis });
        }
    }
    
    public async Task<IActionResult> ChooseLivraison(int id_devis, bool InBoutique = false)
    {
        try
        {
            var commsInLivr = await _context.VvolDevisLivraison
                .Include(q => q.CommandeDevis)
                .Include(q => q.Vol)
                .Where(q => q.IdDevis == id_devis && q.Vol.DateArriver!=null && !q.InAdresse)
                .OrderBy(q => q.Vol.DateDepart).ToListAsync();

            if (!commsInLivr.Any())
            {
                throw new Exception("Aucun élément n'est prêt.");
            }
        
            var boutiques = await BoutiqueService.AllBoutique(_context);

            ViewBag.id_devis = id_devis;
            ViewBag.InBoutique = InBoutique;
            ViewBag.commsInLivr = commsInLivr;
        
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            return View(boutiques);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(OptionImportation) , new { id_devis });
        }
        
    } 

    [CheckClient]
    [HttpPost]
    public async Task<ActionResult> ChooseLivraison(int id_devis ,string? IdBoutique, Coordonner? coordonner)
    {
        try
        {
            await DevisService.ChooseLivraison(_context, IdBoutique, coordonner, id_devis);
            return RedirectToAction(nameof(OptionImportation), new { id_devis });
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            bool InBoutique =  coordonner.Type == 0;
            return RedirectToAction(nameof(ChooseLivraison) , new { id_devis, InBoutique });
        }
    }
    
    public async Task<JsonResult> NotifDevis()
    {
        int res = 0;
        string color = "text-danger";
        try
        {
            if (Request.Cookies[KeyStorage.KeyClient] == null) throw new Exception("Pas de compte");
            Client client = JsonConvert.DeserializeObject<Client>(Request.Cookies[KeyStorage.KeyClient]);
            
            var devs = await _context.VDevisInfos.Where(q => q.DateDelete == null &&  !q.IsLivrer && q.IdClient ==client.Id )
                .ToListAsync();

            foreach (var d in devs)
            {
                switch (d)
                {
                    case { IsLivrer: true}: // Livrer
                        break;
                    case { DatePayer: not null, DateDelete: null, IsLivrer: false }:
                        res += 1;
                        color = "text-warning"; // Payer
                        break;
                    case { DateDelete: not null }: // Delete
                        break;
                    case { DateValidation: not null, DateDelete: null, DatePayer: null, IsLivrer: false }: // Valider
                        res += 1;
                        color = "text-warning";
                        break;
                    case { DateEnvoi: not null, DateValidation: null, DateDelete: null, DatePayer: null, IsLivrer: false }: //Envoyer
                        res += 1;
                        color = "text-danger";
                        break;
                    case { DateEnvoi: null }: // NonEnvoyer
                        break;
                }
            }
            
            return Json( new {res , color});
        }
        catch (Exception e)
        {
            return Json( new {res , color});
        }
    }

}