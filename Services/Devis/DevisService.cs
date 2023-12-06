using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Devis;
using originalstoremada.Models.Devis.Others;
using originalstoremada.Models.Devis.Views;
using originalstoremada.Models.Others;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Users;
using originalstoremada.Services.Mail;
using originalstoremada.Services.Produits;
using originalstoremada.Services.RedirectAttribute;
using originalstoremada.Services.Repo;

namespace originalstoremada.Services.Devis;

public class DevisService: ServiceRepo<VCommanceDevis>
{
    public override Pagination<VCommanceDevis> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }
    public static double[] TranchesData = {25,35,40};
    public static int[] TypePay = { 0, 1};// 0: un, 1: tous

    public static string NonEnvoyer = "Demande non envoyer";
    public static string Livrer = "livrer";
    public static string Delete = "delete";
    public static string Valider = "valider";
    public static string Envoyer = "envoyer (une confirmation est en attente)";
    public static string Payer = "Versement complet déjà effectué.";


    static IQueryable<VDevisInfo> DevisQuery(ApplicationDbContext context, HttpContext httpContext, int type = 0)
    {
        IQueryable<VDevisInfo> queryable = context.VDevisInfos
            .Include(q => q.Client);

        if (type == 0) queryable = queryable.Where(q =>q.DateDelete == null) ;

        if (type == 2) queryable = queryable.Where(q => q.DateEnvoi != null && q.DateValidation == null && q.DateDelete == null );

        if (type == 3) queryable = queryable.Where(q => q.DateValidation != null && q.DatePayer == null && q.DateDelete == null);

        if (type == 4) queryable = queryable.Where(q => q.DatePayer!=null);
        
        if (type == 5) queryable = queryable.Where(q => q.DateDelete!=null);
        
        if (type == 6) queryable = queryable.Where(q => q.IsLivrer);
        return queryable;
    }

    public static  IQueryable<VDevisInfo> MyDevis(ApplicationDbContext context, HttpContext httpContext, int type = 0, bool isAll = false)
    {
        IQueryable<VDevisInfo> queryable = DevisQuery(context, httpContext, type);
        if (!isAll)
        {
            Client client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);
            queryable = queryable.Where(q => q.IdClient == client.Id);
        }
        queryable = queryable.OrderByDescending(q => q.Id);
        return queryable;
    }

    public static void CreateSimulateCommandeDevis( HttpContext httpContext , CommandeDevis commandeDevis)
    {
        try
        {
            List<CommandeDevis> commds = JsonConvert.DeserializeObject<List<CommandeDevis>>(httpContext.Request.Cookies[KeyStorage.CommandeDevis]);
            CommandeDevis last = commds.Last();
            if (commandeDevis.IdDevis<=0) commandeDevis.IdDevis = last.IdDevis+1;
            else commandeDevis.IdDevis = last.IdDevis;
            if(commds.Count<=0) commandeDevis.Id = last.Id;
            else commandeDevis.Id = last.Id + 1;
            
            commds.Add(commandeDevis);
            httpContext.Response.Cookies.Append(KeyStorage.CommandeDevis, JsonConvert.SerializeObject(commds), CookieFunction.OptionDay(30));
        }
        catch (Exception e)
        {
            if (httpContext.Request.Cookies[KeyStorage.CommandeDevis] != null)
                httpContext.Response.Cookies.Delete(KeyStorage.CommandeDevis);
            List<CommandeDevis> commds = new List<CommandeDevis>();
            commandeDevis.Id = 1;
            commandeDevis.IdDevis = 1;
            commds.Add(commandeDevis);
            httpContext.Response.Cookies.Append(KeyStorage.CommandeDevis, JsonConvert.SerializeObject(commds),CookieFunction.OptionDay(30));
        }
    }

    public static void DeleteSimulateCommandeDevis(HttpContext httpContext,int? id,  int? IDevis, bool isAll)
    {
        try
        {
            List<CommandeDevis> commds = JsonConvert.DeserializeObject<List<CommandeDevis>>(httpContext.Request.Cookies[KeyStorage.CommandeDevis]);
            if (isAll)
            {
                commds.RemoveAll(q => q.IdDevis == IDevis);
            }
            else
            {
                foreach (var q in commds)
                {
                    if (q.Id == id)
                    {
                        commds.Remove(q);
                        break;
                    }
                }
            }
            httpContext.Response.Cookies.Delete(KeyStorage.CommandeDevis);
            if(commds.Count>0)
                httpContext.Response.Cookies.Append(KeyStorage.CommandeDevis, JsonConvert.SerializeObject(commds),CookieFunction.OptionDay(30));
        }
        catch (Exception e)
        {
            if (httpContext.Request.Cookies[KeyStorage.CommandeDevis] != null)
                httpContext.Response.Cookies.Delete(KeyStorage.CommandeDevis);
        }
    }

    public static void UpdateSimulateCommandeDevis(HttpContext httpContext, CommandeDevis commandeDevis)
    {
        try
        {
            List<CommandeDevis> comms = JsonConvert.DeserializeObject<List<CommandeDevis>>(httpContext.Request.Cookies[KeyStorage.CommandeDevis]);
            foreach (var q in comms)
            {
                if (q.Id == commandeDevis.Id)
                {
                    q.Couleur = commandeDevis.Couleur;
                    q.Nombre = commandeDevis.Nombre;
                    q.Taille = commandeDevis.Taille;
                    q.PrixEuro = commandeDevis.PrixEuro;
                    q.ProduitName = commandeDevis.ProduitName;
                    q.ReferenceSite = commandeDevis.ReferenceSite;
                    break;
                }
            }
            
            httpContext.Response.Cookies.Append(KeyStorage.CommandeDevis, JsonConvert.SerializeObject(comms),CookieFunction.OptionDay(30));
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    public static async Task<List<VDevisInfo>> DevisSimulates(ApplicationDbContext context ,HttpContext httpContext)
    {
        List<VDevisInfo> res = new List<VDevisInfo>();
        try
        {
            List<CommandeDevis> comms = JsonConvert.DeserializeObject<List<CommandeDevis>>(httpContext.Request.Cookies[KeyStorage.CommandeDevis]);
            var C = comms.GroupBy(q => q.IdDevis)
                .Select(group => new
                {
                    Id = group.Key,
                    TotalEuro = Math.Round(group.Sum(m => m.PrixEuro * m.Nombre),2) ,
                    Quantiter = group.Sum(m => m.Nombre)
                });
            var coursEUro = await context.CoursEuro.OrderByDescending(q => q.Date).FirstOrDefaultAsync();
            foreach (var q in C)
            {
                res.Add(new VDevisInfo()
                {
                    Id = (int)q.Id, TotalQuantiter = q.Quantiter, TotalPrixEuro = q.TotalEuro,
                    Date = DateTimeToUTC.Make(DateTime.Now), 
                    EtatActuel = new EtatDevis()
                    {
                        Etat = "brouillon", Color = "danger"
                    },
                    CoursEuro = coursEUro.MontantAriary ,
                    TotalPrixAriary1 = Math.Round( q.TotalEuro * coursEUro.MontantAriary ,2)
                });
            }
        }
        catch (Exception e)
        {
            // ignored
        }
        return res;
    }

    public static async Task<VDevisInfo> GetDevisBySimulate(ApplicationDbContext context, HttpContext httpContext, long? id_devis)
    {
        List<VDevisInfo> devis = await DevisSimulates(context, httpContext);
        try
        {
            return devis.FirstOrDefault(q => q.Id == id_devis);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static async Task<List<VDevisInfo>> MelangeMyDevis(ApplicationDbContext context ,  HttpContext httpContext, List<VDevisInfo> ? myDevis, int pagId, int type = 0)
    {
        List<VDevisInfo> res = new List<VDevisInfo>();
        if (type != 1)
        {
            if (myDevis != null)
            {
                res.AddRange(myDevis);
            }
        }

        if (type == 0 || type == 1)
        {
            if (pagId == 1 || pagId == 0)
            {
                List<VDevisInfo>? brouillons = await DevisSimulates( context, httpContext);
                if(brouillons!=null) res.AddRange(brouillons);
            }
        }
        
        if (res != null && res.Count > 1) res = res.OrderByDescending(q => q.Date).ToList();
        return res;
    }

    public static List<CommandeDevis> DevisSimulateByIdDevis(HttpContext httpContext ,long? IdDevis)
    {
        List<CommandeDevis> comms = JsonConvert.DeserializeObject<List<CommandeDevis>>(httpContext.Request.Cookies[KeyStorage.CommandeDevis]);
        List<CommandeDevis> res = comms.Where(q => q.IdDevis == IdDevis).ToList();
        return res;
    }

    [CheckClient]
    public static void DemanderDevis(ApplicationDbContext context, HttpContext httpContext ,long? IdDevis)
    {
        try
        {
            List<CommandeDevis> All = JsonConvert.DeserializeObject<List<CommandeDevis>>(httpContext.Request.Cookies[KeyStorage.CommandeDevis]);
            List<CommandeDevis> commandeDevis = DevisSimulateByIdDevis(httpContext, IdDevis);
            if (commandeDevis.Count > 0)
            {
                Client client = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);
                Models.Devis.Devis devis = new Models.Devis.Devis()
                {
                    IdClient = client.Id,
                    Date = DateTimeToUTC.Make(DateTime.Now),
                    DateEnvoi = DateTimeToUTC.Make(DateTime.Now)
                };
                context.Add(devis);
                context.SaveChanges();
                foreach (var q in commandeDevis)
                {
                    CommandeDevis C = new CommandeDevis()
                    {
                        IdDevis = devis.Id,
                        IdCategorie = q.IdCategorie,
                        Couleur = q.Couleur,
                        Taille = q.Taille,
                        Nombre = q.Nombre,
                        PrixEuro = q.PrixEuro,
                        ProduitName = q.ProduitName,
                        ReferenceSite = q.ReferenceSite
                    };
                    context.Add(C);
                }
                All.RemoveAll(q => q.IdDevis == IdDevis);
                httpContext.Response.Cookies.Delete(KeyStorage.CommandeDevis);
                if(All.Count>0) 
                    httpContext.Response.Cookies.Append(KeyStorage.CommandeDevis, JsonConvert.SerializeObject(All),CookieFunction.OptionDay(30));
                context.SaveChanges();
            }
        }
        catch (Exception e)
        {
            // ignored
            Console.WriteLine(e.Message);
        }
    } 

    public static async Task CreateCommandeDevis(ApplicationDbContext context, CommandeDevis commandeDevis, Client client)
    {
        if (commandeDevis.IdDevis == 0)
        {
            Models.Devis.Devis devis = await CreateGEtDevis(context, client);
            commandeDevis.IdDevis = devis.Id;
        }
        else
        {
            Models.Devis.Devis d = await context.Devis.FindAsync(commandeDevis.IdDevis);
            if(d.DateValidation!=null) return;
        }
        
        context.Add(commandeDevis);
        await context.SaveChangesAsync();
    }

    static async Task<Models.Devis.Devis> CreateGEtDevis(ApplicationDbContext context, Client client)
    {
        var devis = new Models.Devis.Devis
        {
            IdClient = client.Id,
            Date = DateTimeToUTC.Make(DateTime.Now)
        };
        context.Add(devis);
        await context.SaveChangesAsync();
        return devis;
    }

    public static void SetEtatDevis(VDevisInfo devisInfo)
    {
        string[] ET = GetEtatDevis(devisInfo);
        devisInfo.EtatActuel = new EtatDevis()
        {
           Etat = ET[0], Color = ET[1]
        }; 
    }

    public static List<VDevisInfo> SetAllEtatDevis(List<VDevisInfo> devisInfos)
    {
        foreach (var q in devisInfos)
            SetEtatDevis(q);
        return devisInfos;
    }

    public static void SetEtatPrixDevis(VDevisInfo devisInfos)
    {
        if (devisInfos.EtatActuel.Etat == Envoyer)
        {
            devisInfos.ActuelEuroElement = devisInfos.TotalPrixEuro;
            devisInfos.ActuelEuro = devisInfos.TotalPrixFinEuro;
            devisInfos.ActuelAriaryElement = devisInfos.TotalPrixAriary1;
            devisInfos.ActuelAriary = devisInfos.TotalPrixFinAriary1;
        }
        else if (devisInfos.EtatActuel.Etat != Envoyer)
        {
            devisInfos.ActuelEuroElement = devisInfos.TotalPrixEuro;
            devisInfos.ActuelEuro = devisInfos.TotalPrixFinEuro;
            devisInfos.ActuelAriaryElement = devisInfos.TotalPrixAriaryReel;
            devisInfos.ActuelAriary = devisInfos.TotalPrixFinAriaryReel;
        }
    }

    public static string[] GetEtatDevis(VDevisInfo devis)
    {
        string[] res = new string[2];
        switch (devis)
        {
            case { IsLivrer: true}:
                res = new string[] { Livrer , "success" };
                break;
            case { DatePayer: not null, DateDelete: null, IsLivrer: false }:
                res = new string[] { Payer , "success" };
                break;
            case { DateDelete: not null }:
                res = new string[] { Delete , "secondary" };
                break;
            case { DateValidation: not null, DateDelete: null, DatePayer: null, IsLivrer: false }:
                res = new string[] { Valider , "warning" };
                break;
            case { DateEnvoi: not null, DateValidation: null, DateDelete: null, DatePayer: null, IsLivrer: false }:
                res = new string[] { Envoyer , "danger" };
                break;
            case { DateEnvoi: null }:
                res = new string[] { NonEnvoyer , "danger" };
                break;
            default:
                res = new string[] { "" , "secondary" };
                break;
        }

        return res;
    }

    public static Dictionary<int, string> SuivisDevisEtat(VDevisInfo devis)
    {
        Dictionary<int, string> res = new Dictionary<int, string>();
        for(int i=0;i<= 5 ;i++) res.Add(i, "");
        if (devis.EtatActuel.Etat == Livrer)
        {
            res[1] = "completed";
            res[2] = "completed";
            res[3] = "completed";
            res[4] = "completed active";
        } 
        else if (devis.CoursDevis != null)
        {
            res[1] = "completed";
            res[2] = "completed";
            res[3] = "completed";
            res[4] = "active";
        }else if (devis.CoursDevis  == null)
        {
            res[1] = "completed";
            res[2] = "completed";
            res[3] = "active";
        }  else if (devis.EtatActuel.Etat == Valider)
        {
            res[1] = "completed";
            res[2] = "completed";
            res[3] = "active";
        } else if (devis.EtatActuel.Etat == Envoyer)
        {
            res[1] = "completed";
            res[2] = "active";
        }
        
        if (devis.EtatActuel.Etat == Delete)
        {
            res[0] = "incompleted active";
        }

        return res;
    }

    public static async  Task<List<VCommanceDevis>> Commandes(ApplicationDbContext context ,long? id_devis)
    {
        var commandes = await context.VCommanceDevis
            .Include(q => q.Devis)
            .Include(q => q.CategorieProduit)
            .Where(q => q.IdDevis == id_devis)
            .ToListAsync();
        return commandes;
    }

    public static Tranche[] MontantPayementDiviser(VDevisInfo devis, List<PayementDevis> payementDevis, bool ver)
    {
        double total = devis.CoursDevis !=null? devis.TotalPrixFinAriaryReel : devis.TotalPrixFinAriary1;
        Tranche[] tranches = new Tranche[TranchesData.Length];
        double[] montants = new double[TranchesData.Length];
        Array.Sort(TranchesData);
        for (int i=0;i< montants.Length;i++)
        {
            montants[i] = total * TranchesData[i] / 100;
        }
        double sommeMontants = montants.Sum();
        double ecart = total - sommeMontants;
        for (int i = 0; i < montants.Length; i++)
        {
            double ajustement = ecart / (montants.Length - i);
            montants[i] += ajustement;
            montants[i] = Math.Round(montants[i], 2);
            ecart -= ajustement;
        }

        if (ver)
            payementDevis = payementDevis.OrderBy(q => q.Montant).ToList();
        for (int i = 0; i < TranchesData.Length; i++)
        {
            tranches[i] = new Tranche(montants[i], TranchesData[i]);
            if (ver)
            {
                try
                {
                    var P = payementDevis[i];
                    if (P.DatePayement == null)
                    {
                        tranches[i].EstPayeeSimulate = true;
                        tranches[i].EstPayee = false;
                    }
                    else
                    {
                        tranches[i].EstPayeeSimulate = true;
                        tranches[i].EstPayee = true;
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
        return tranches;
    }

    public static Dictionary<string, List<Tranche>> GetNextAndRestePayement(Tranche[] tranches)
    {
        Dictionary<string, List<Tranche>> res = new Dictionary<string, List<Tranche>>();
        List<Tranche> nexts = new List<Tranche>();
        List<Tranche> restes = new List<Tranche>();
        bool nextPayer = false;
        
        for (int i = 0; i < tranches.Length; i++)
        {
            if (!tranches[i].EstPayee && nextPayer==false)
            {
                restes.Add(tranches[i]);
                if(nextPayer == false) nexts.Add(tranches[i]);
                nextPayer = true;
            }
            
        }

        if (nexts.Count() > 0 && restes.Count > 0)
        {
            res.Add("next", nexts);
            res.Add("restes", restes);
        }
        
        return res;
    }

    public static async Task Payement(ApplicationDbContext context, HttpContext httpContext, SmtpConfig smtpConfig , long id_devis, int indice, int typePayement)
    {
        Client C = JsonConvert.DeserializeObject<Client>(httpContext.Request.Cookies[KeyStorage.KeyClient]);
        Client client = await context.Client.FindAsync(C.Id);
        var devis = await context.VDevisInfos.FirstOrDefaultAsync(q => q.Id == id_devis);
        if(devis.DateDelete != null) throw new Exception("Devis invalide");
        if (devis.DatePayer != null) throw new Exception("Payement Deja effectuer");
        List<PayementDevis> payements = await context.PayementDevis.OrderByDescending(q => q.Date).Where(q => q.IdDevis == id_devis).ToListAsync();
        if(payements.Count() > 0 && payements[payements.Count() - 1].DatePayement == null) throw new Exception("Tous les Payements en simulation sont tous déjà fait (en attente de confirmation)");
        if(payements.Count >= TranchesData.Length) throw new Exception("Payement Deja effectuer");
        
        if (payements.Count <= 0)
        {
            Models.Devis.Devis dev = await context.Devis.FindAsync(id_devis);
            dev.CoursDevis = devis.CoursEuro;
            dev.DateEnvoi = DateTimeToUTC.Make((DateTime)dev.DateEnvoi);
            dev.DateValidation = DateTimeToUTC.Make((DateTime)dev.DateValidation);
            dev.Date = DateTimeToUTC.Make(dev.Date);
            
            var comms = await context.CommandeDevis
                .Where(q => q.IdDevis == id_devis)
                .Join(context.VCommanceDevis, 
                    cmd => cmd.Id, 
                    vcmd => vcmd.Id, 
                    (cmd, vcmd) => new { Commande = cmd, VCommance = vcmd })
                .ToListAsync();

            foreach (var item in comms)
            {
                var Q = item.Commande;
                var cc = item.VCommance;

                Q.FraisImportationReel = cc.FraisImpoMontant;
                Q.CommissionReel = cc.CommissionMontant;
            }

            context.UpdateRange(comms.Select(c => c.Commande));

            context.Update(dev);
        }
        VPayementDevisEtat? payInf = await context.VPayementDevisEtat.FirstOrDefaultAsync(q => q.IdDevis == id_devis);
        Tranche[] tranches = MontantPayementDiviser(devis, payements, false);

        double sumMess = 0;
        if (indice == 0)
        {
            Tranche T = tranches[payements.Count()];
            sumMess += T.Valeur;
            PayementDevis P = new PayementDevis()
            {
                IdDevis = id_devis,
                IdClient = client.Id,
                Montant = T.Valeur ,
                IdTypePayement = typePayement,
                Date = DateTimeToUTC.Make(DateTime.Now)
            };
            /*if (payements.Count + 1 == tranches.Length)
            {
                Models.Devis.Devis D = await context.Devis.FirstOrDefaultAsync(q => q.Id == id_devis);
                D.DatePayer = DateTimeToUTC.Make(DateTime.Now);
                D.Date = DateTimeToUTC.Make(D.Date);
                D.DateEnvoi = DateTimeToUTC.Make((DateTime)D.DateEnvoi);
                D.DateValidation = DateTimeToUTC.Make((DateTime)D.DateValidation);
                context.Update(D);
            }*/
            context.Add(P);
        } else if (indice == 1)
        {
            DateTime dat = DateTimeToUTC.Make(DateTime.Now);
            for (int i = payements.Count; i < tranches.Length; i++)
            {
                PayementDevis P = new PayementDevis()
                {
                    IdDevis = id_devis,
                    IdClient = client.Id,
                    Montant = tranches[i].Valeur ,
                    IdTypePayement = typePayement,
                    Date = dat
                };
                context.Add(P);
                sumMess += tranches[i].Valeur;
            }
            /*Models.Devis.Devis D = await context.Devis.FirstOrDefaultAsync(q => q.Id == id_devis);
            D.DatePayer = DateTimeToUTC.Make(DateTime.Now);
            D.Date = DateTimeToUTC.Make(D.Date);
            D.DateEnvoi = DateTimeToUTC.Make((DateTime)D.DateEnvoi);
            D.DateValidation = DateTimeToUTC.Make((DateTime)D.DateValidation);
            context.Update(D);*/
        }
        
        else throw new Exception("Error");

        string body = $"Votre paiement de {@Formattage.Valeur(sumMess)} Ariary a été effectué avec succès. Merci de votre achat.";

        await context.SaveChangesAsync();
        // MailService.EnvoyerEmail(smtpConfig,"mahefanant@gmail.com", client.Mail,"OSMada (Confirmation de Payement)", body);
    }

    public static async Task ChooseLivraison(ApplicationDbContext context , string? IdBoutique, Coordonner? coordonner, long id_devis)
    {
        Boutique? bout;
        int? id_b;
        
        bout = await PayementShopService.ChooseBoutique(context, coordonner, IdBoutique);
        id_b = bout.Id;
        coordonner.IdBoutique = (int)id_b;

        bool isInBoutique = coordonner.Type == 0;
        double frais = 0;
        if (coordonner.Type == 1)
        {
            frais = coordonner.Distance * PayementShopService.PrixParKm; 
            if (frais < PayementShopService.PrixMinKm) frais = PayementShopService.PrixMinKm;
        }
        else
        {
            coordonner.Ville = bout.Ville;
            coordonner.Quartier = bout.Quartier;
            coordonner.Longitude = bout.Longitude;
            coordonner.Latitude = bout.Latitude;
        }
            
        frais = Math.Round(frais, 2);
            
        var commsInLivr = await context.VvolDevisLivraison
            .Include(q => q.CommandeDevis)
            .Include(q => q.Vol)
            .Where(q => q.IdDevis == id_devis && q.Vol.DateArriver!=null && !q.InAdresse)
            .OrderBy(q => q.Vol.DateDepart).ToListAsync();

        foreach (var c in commsInLivr)
        {
            var adresse = new AdresseLivraisonDevis()
            {
                IdDevis = id_devis, Ville = coordonner.Ville , Quartier = coordonner.Quartier , Longitude = coordonner.Longitude, 
                Latitude = coordonner.Latitude, IsInBoutque =isInBoutique, FraisLivraison = frais ,
                IdCommande = c.IdCommande , Quantiter = c.Quantiter, IdVolDevis = c.Id
            };
            context.Add(adresse);
        }
            
        await context.SaveChangesAsync();
    }

    public static PayementDevis PayementNonConfirmerForm(VDevisInfo devisInfo, List<PayementDevis> payementDevis, List<TypePayement> typePayements)
    {
        if (!devisInfo.EtatPayement && payementDevis.Any() && typePayements.Any())
        {
            List<PayementDevis> pays = payementDevis.Where(q => q.DatePayement == null).ToList();
            double sum = Math.Round(pays.Sum(q => q.Montant), 2);
            PayementDevis P = new PayementDevis()
            {
                IdDevis = pays[0].IdDevis, IdClient = pays[0].IdClient, IdTypePayement = pays[0].IdTypePayement,
                Date = pays[0].Date, Montant = sum,
                TypePayement = typePayements.FirstOrDefault(q => q.Id == pays[0].IdTypePayement)
            };
            return P;
        }
        return null;
    }
}