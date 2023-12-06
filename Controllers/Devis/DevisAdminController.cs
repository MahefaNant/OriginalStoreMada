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
using originalstoremada.Models.StatAdmin;
using originalstoremada.Services;
using originalstoremada.Services.Devis;
using originalstoremada.Services.RedirectAttribute;
using TimeZoneConverter;

namespace originalstoremada.Controllers.Devis;

public class DevisAdminController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public DevisAdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> Index(int? pagId, bool IsPag = false,int type =0)
    {
        try
        {
            
            if (!IsPag)
            {
                HttpContext.Session.SetInt32("typeRechDev", type);
            }
            else
            {
                type = (int)HttpContext.Session.GetInt32("typeRechDev");
            }
            
            IQueryable<VDevisInfo> devis = DevisService.MyDevis(_context, HttpContext, type, true);
            if (!IsPag) pagId = 1;
            Pagination<VDevisInfo> pagination = new Pagination<VDevisInfo>(10, pagId, devis);
            devis = pagination.Paginate();
            
            var res = await devis.ToListAsync();
            if(res.Any()) DevisService.SetAllEtatDevis(res);

            ViewBag.devis = res;
            
            ViewBag.paginations = pagination;
            ViewBag.type = type;
        }
        catch (Exception e)
        {
            // ignored
        }
        if (TempData.TryGetValue("error", out var error))
        {
            ViewBag.error = error;
        }

        return View();
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> Details(long? id_devis)
    {
       List<VCommanceDevis> commandeDevis = new List<VCommanceDevis>();
        try
        {
            var devis =await _context.VDevisInfos.FirstOrDefaultAsync(q => q.Id ==id_devis);
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

    [CheckAdminLevel5]
    [HttpPost]
    public async Task<IActionResult> Valider(long id_devis)
    {
        try
        {
            var devis = await _context.Devis.FindAsync(id_devis);
            devis.DateValidation = DateTimeToUTC.Make(DateTime.Now);
            devis.DateEnvoi = DateTimeToUTC.Make((DateTime)devis.DateEnvoi);
            /*devis.FraisImportation = infos.MaxFraisImpo;
            devis.Commission = infos.MaxCommission;
            devis.CoursDevis = infos.MaxCoursEuro;*/
            devis.Date = DateTimeToUTC.Make(devis.Date);
            _context.Update(devis);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        
        return RedirectToAction(nameof(Index), new { type = 3 });
    }

    [CheckAdminLevel5]
    [HttpPost]
    public async Task<IActionResult> NonValide(long id_devis, string remarque)
    {
        try
        {
            var devis = await _context.Devis.FindAsync(id_devis);
            devis.DateDelete = DateTimeToUTC.Make(DateTime.Now);
            devis.Date = DateTimeToUTC.Make(devis.Date);
            devis.DateEnvoi = DateTimeToUTC.Make((DateTime)devis.DateEnvoi);
            devis.Remarque = remarque;
            if(devis.DateValidation!=null) DateTimeToUTC.Make((DateTime)devis.DateValidation);
            _context.Update(devis);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        
        return RedirectToAction(nameof(Index), new { type = 0 });
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> Payement(long? id_devis)
    {
        List<VCommanceDevis> commandeDevis = new List<VCommanceDevis>();
        try
        {
            var devis =await _context.VDevisInfos.FirstOrDefaultAsync(q => q.Id ==id_devis);
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

    [HttpPost]
    [CheckAdminLevel5]
    public async Task<IActionResult> ConfirmerPayementDevis(long id_devis)
    {
        var payes = await _context.PayementDevis.Where(q => q.IdDevis == id_devis && q.DatePayement == null)
            .ToListAsync();
        if (payes.Any())
        {
            DateTime D = DateTimeToUTC.Make(DateTime.Now);
    
            foreach (var q in payes)
            {
                q.DatePayement = D;
                q.Date = DateTimeToUTC.Make(q.Date);
            }

            _context.UpdateRange(payes);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Payement), new { id_devis });
    }
    
    [HttpPost]
    [CheckAdminLevel5]
    public async Task<IActionResult> FinaliserPayementDevis(long id_devis)
    {
        try
        {
            DateTime D = DateTimeToUTC.Make(DateTime.Now);
            var devis = await _context.Devis.FindAsync(id_devis);
            devis.DatePayer = D;

            devis.Date = DateTimeToUTC.Make(devis.Date);
            if (devis.DateEnvoi != null) devis.DateEnvoi = DateTimeToUTC.Make((DateTime)devis.DateEnvoi);
            if (devis.DateValidation != null) devis.DateValidation = DateTimeToUTC.Make((DateTime)devis.DateValidation);
            if (devis.DateDelete != null) devis.DateDelete = DateTimeToUTC.Make((DateTime)devis.DateDelete);
            _context.Update(devis);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // ignored
            TempData["error"] = e.Message;
        }

        return RedirectToAction(nameof(Payement), new { id_devis });
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> StatIndex(int? Annee, int? pagId, bool isPagination = false)
    {
        try
        {
            if (Annee == null)
            {
                DateTime D = DateTimeToUTC.Make(DateTime.Now);
                Annee = D.Year;
            }
            
            if (!isPagination) pagId = 1;

            IQueryable<VPayementDevisSum> queryPayd = _context.VPayementDevisSum
                .Include(q => q.Client)
                .Include(q => q.TypePayement)
                .OrderByDescending(q => q.Date)
                .Where(q => q.DatePayement != null && q.Date.Year == Annee);

            Pagination<VPayementDevisSum> paginations = new Pagination<VPayementDevisSum>(10, pagId, queryPayd);
            queryPayd = paginations.Paginate();

            var model = await queryPayd.ToListAsync();
            
            List<double> payDatas = new List<double>();
            List<string> labelMois = new List<string>();

            List<VBeneficeDevisParAnsMoisReel> benefs = await _context.VBeneficeDevisParAnsMoisReel
                .Where(q => q.Annee == Annee).ToListAsync();
            
            foreach (var d in benefs)
            {
                labelMois.Add(UtilesFonctions.Mois()[d.Mois - 1]);
                payDatas.Add(d.TotalBenefice);
            }
            
            ViewBag.Annee = Annee;
            ViewBag.isPagination = isPagination;
            ViewBag.pagId = pagId;
            
            ViewBag.labelMois = JsonConvert.SerializeObject(labelMois);
            ViewBag.payDatas = JsonConvert.SerializeObject(payDatas);
            
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
            return RedirectToAction("Login" , "Admin");
        }
    }

    [CheckAdminLevel5]
    public async Task<IActionResult> OptionImportation(long id_devis)
    {
        try
        {
            var commandeReste = await _context.VCommandeDevisResteInVol
                .Where(q => q.IdDevis == id_devis && q.QuantiterReste > 0).ToListAsync();

            var volCommande = await _context.VvolDevisLivraison
                .Include(q => q.CommandeDevis)
                .Include(q => q.Vol)
                .Where(q => q.IdDevis == id_devis)
                .OrderBy(q => q.Vol.DateDepart).ToListAsync();
            
            var vols = await _context.Vol
                .Where(q => q.DateDepart > DateTime.UtcNow && q.DateArriver == null)
                .OrderBy(q => q.DateDepart)
                .ToListAsync();
            
            var commsInLivr = volCommande
                .Where(q => q.Vol.DateArriver != null).ToList();

            ViewBag.aLivrer = 0;

            if (commsInLivr.Any())
            {
                var aLivrer = commsInLivr.Count(q => q.DatePret == null && q.DateLivrer == null && q.IsInBoutque!=null && (bool)!q.IsInBoutque);
                ViewBag.aLivrer = aLivrer;
                
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
            
            ViewBag.volCommande = volCommande;
            ViewBag.vols = vols;
            ViewBag.id_devis = id_devis;
            ViewBag.commsInLivr = commsInLivr;
            
            if (TempData.TryGetValue("error", out var error))
            {
                ViewBag.error = error;
            }
            
            return View(commandeReste);
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(Details) , new { id_devis });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SetOptionImportation(long id_devis, long id_commande, int quantiter, long id_vol, long id_vol2 = 0, int etat = 0)
    {
        try
        {
            if(etat< 0 || etat > 1) throw new Exception("Une Erreur est survenue!");
            var vol = await _context.Vol.FindAsync(id_vol);
            if (vol == null) throw new Exception("Cette Vol n'existe pas!");

            if(vol.DateDepart < DateTime.Now) throw new Exception("On ne peut plus choisir ce vol!( Date noncohérent )");

            var comm = await _context.VCommandeDevisResteInVol
                .FirstOrDefaultAsync(q => q.IdDevis == id_devis && q.Id == id_commande);

            var vol_devis = await _context.VolDevis.Where(q => q.IdCommande == id_commande && q.IdVol == id_vol)
                .FirstOrDefaultAsync();
            
            if (etat == 0)
            {
                if (vol_devis != null)
                {
                    if (comm.QuantiterReste + quantiter > comm.Nombre) throw new Exception("Quantités insuffisant");
                    vol_devis.Quantiter = comm.QuantiterReste + quantiter;
                    _context.Update(vol_devis);
                }
                else
                {
                    if (quantiter > comm.QuantiterReste) throw new Exception("Quantités insuffisant");
                    vol_devis = new VolDevis()
                    {
                        IdDevis = id_devis , IdCommande = id_commande ,IdVol = id_vol , Quantiter = quantiter
                    };
                    _context.Add(vol_devis);
                }
            } else
            {
                if (quantiter > comm.Nombre) throw new Exception("Quantités insuffisant");
                vol_devis.Quantiter = quantiter;
                vol_devis.IdVol = id_vol2;
                _context.Update(vol_devis);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(OptionImportation) , new { id_devis });
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            return RedirectToAction(nameof(OptionImportation) , new { id_devis });
        }
    }

    [HttpPost]
    [CheckAdminLevel5]
    public async Task<IActionResult> SetCommandePret(long id_devis, DateTime dateEstim)
    {
        try
        {
            var adresses = await _context.AdresseLivraisonDevis
                .Where(q => q.IdDevis == id_devis && q.DatePret == null && q.DateLivrer == null && !q.IsInBoutque)
                .ToListAsync();

            foreach (var a in adresses)
            {
                a.DatePret = DateTimeToUTC.Make(DateTime.Now);
                a.DateEstimation = DateTimeToUTC.Make(dateEstim);
                _context.Update(a);
            }
            
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
        }
        
        return RedirectToAction(nameof(OptionImportation) , new { id_devis });
    }
    
    [HttpPost]
    [CheckAdminLevel5]
    public async Task<IActionResult> ConfirmLivrer(long id_devis, long id_commande, long id_voldevis)
    {
        try
        {
            var a = await _context.AdresseLivraisonDevis
                .FirstOrDefaultAsync(q => q.IdDevis == id_devis && q.IdCommande == id_commande && q.IdVolDevis == id_voldevis);


            if(a.DatePret!=null) a.DatePret = DateTimeToUTC.Make((DateTime)a.DatePret);
            if(a.DateEstimation!=null) a.DateEstimation = DateTimeToUTC.Make((DateTime)a.DateEstimation);
            a.DateLivrer = DateTimeToUTC.Make(DateTime.Now);
            _context.Update(a);
            
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            TempData["error"] = e.Message;
            Console.WriteLine(e.Message);
        }
        
        return RedirectToAction(nameof(OptionImportation) , new { id_devis });
    }
    
    public async Task<JsonResult> NotifDevis()
    {
        int res = 0;
        string color = "text-danger";
        try
        {
            var devs = await _context.VDevisInfos.Where(q => q.DateDelete == null &&  !q.IsLivrer)
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