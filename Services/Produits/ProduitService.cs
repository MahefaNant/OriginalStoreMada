using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.CSV;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.views;
using originalstoremada.Services.Produits.Others;
using originalstoremada.Services.Repo;

namespace originalstoremada.Services.Produits;

public class ProduitService: ServiceRepo<Produit>
{

    public static readonly string SessionNameRechercheAdmin = "rechercheProduitAdmin";
    
    public override Pagination<Produit> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }
    
    public ProduitService(ApplicationDbContext context)
    {
        _context = context;
    }

    public static IQueryable<VImagePrincipalPrixProduit> AllProduits(ApplicationDbContext context,HttpContext httpContext, RechercheProduitAdmin? recherche, bool refresh, bool isPagination)
    {
        IQueryable<VImagePrincipalPrixProduit> res = context.VImagePrincipalPrixProduit.Include(p => p.CategorieProduit);
        if (refresh)
        {
            if(httpContext.Session.GetString(SessionNameRechercheAdmin)!=null) httpContext.Session.Remove(SessionNameRechercheAdmin);
        }
        if (isPagination && !refresh)
        {
            if (httpContext.Session.GetString(SessionNameRechercheAdmin) != null)
            {
                try
                {
                    recherche = JsonConvert.DeserializeObject<RechercheProduitAdmin>(httpContext.Session.GetString(SessionNameRechercheAdmin));
                }
                catch (Exception e)
                {
                    httpContext.Session.Remove(SessionNameRechercheAdmin);
                }
            }
            else recherche = null;
        }

        if (recherche != null&& !refresh)
        {
            if (!recherche.Nom.IsNullOrEmpty() || recherche.IdCategorie != 0 || recherche.IdType != 0 || recherche.minPrice != 0 ||
                recherche.maxPrice != null)
            {
                res = FindNom(res, recherche);
                res = FindCategorie(res, recherche);
                res = FindType(res, recherche);
                res = FindPrix(res, recherche);
                Console.WriteLine("rech: " +JsonConvert.SerializeObject(recherche));
                httpContext.Session.SetString(  SessionNameRechercheAdmin,JsonConvert.SerializeObject(recherche));
            }
        }
        return res;
    }
    
    /*---------------------RECHERCHE--------------------*/
    

    static IQueryable<VImagePrincipalPrixProduit> FindNom(IQueryable<VImagePrincipalPrixProduit> produits, RechercheProduitAdmin? recherche)
    {
        if (recherche!=null && !recherche.All && !recherche.Nom.IsNullOrEmpty())
        {
            produits = produits.Where(q => q.Nom.Contains(recherche.Nom));
        }
        return produits;
    }
    
    static IQueryable<VImagePrincipalPrixProduit> FindCategorie(IQueryable<VImagePrincipalPrixProduit> produits, RechercheProduitAdmin? recherche)
    {
        if (recherche!=null &&!recherche.All && recherche.IdCategorie != 0)
        {
            produits = produits.Where(q => q.IdCategorie == recherche.IdCategorie);
        }
        return produits;
    }
    
    static IQueryable<VImagePrincipalPrixProduit> FindType(IQueryable<VImagePrincipalPrixProduit> produits, RechercheProduitAdmin? recherche)
    {
        if (recherche!=null &&!recherche.All && recherche.IdType != 0)
        {
            produits = produits.Where(q => q.IdType == recherche.IdType);
        }
        return produits;
    }
    
    static IQueryable<VImagePrincipalPrixProduit> FindPrix(IQueryable<VImagePrincipalPrixProduit> produits, RechercheProduitAdmin? recherche)
    {
        if (recherche!=null &&!recherche.All && recherche.maxPrice != null && recherche.maxPrice>0)
        {
            produits = produits.Where(q => q.PrixVenteInitial >= recherche.minPrice && q.PrixVenteInitial <= recherche.maxPrice);
        }
        return produits;
    }

    static IQueryable<VImagePrincipalPrixProduit> OrderAllProduits(IQueryable<VImagePrincipalPrixProduit> produits, RechercheProduitAdmin? recherche)
    {
        if (recherche!=null &&recherche.TypeOrder == "asc")
        {
            if (recherche.OrderBy.Equals("designation")) produits = produits.OrderBy(q => q.Nom);
            if (recherche.OrderBy.Equals("categorie")) produits = produits.OrderBy(q => q.categorie);
            if (recherche.OrderBy.Equals("prix")) produits = produits.OrderBy(q => q.PrixVenteInitial);
        }
        else if(recherche.TypeOrder == "desc")
        {
            if (recherche.OrderBy.Equals("designation")) produits = produits.OrderByDescending(q => q.Nom);
            if (recherche.OrderBy.Equals("categorie")) produits = produits.OrderByDescending(q => q.categorie);
            if (recherche.OrderBy.Equals("prix")) produits = produits.OrderByDescending(q => q.PrixVenteInitial);
        }
        return produits;
    }
    
    /*-------------------------------------------------------*/
    
    public static async Task SaveFraisImportation(ApplicationDbContext context ,FraisImportation fraisImportation)
    {
        var lastFrais = await context.FraisImportation
            .Where(q => q.IdCategorie == fraisImportation.IdCategorie && q.DateFin == null && q.DateDeb < DateTimeToUTC.Make((DateTime)fraisImportation.DateDeb))
            .OrderByDescending(q => q.DateDeb)
            .FirstOrDefaultAsync();
        if (lastFrais != null)
        {
            lastFrais.DateDeb = DateTimeToUTC.Make(lastFrais.DateDeb);
            lastFrais.DateFin = DateTimeToUTC.Make(fraisImportation.DateDeb);
            context.Update(lastFrais);
        }
        fraisImportation.DateDeb = DateTimeToUTC.Make(fraisImportation.DateDeb);
        if (fraisImportation.DateFin != null) fraisImportation.DateFin = DateTimeToUTC.Make((DateTime)fraisImportation.DateFin);
        context.Add(fraisImportation);
        await context.SaveChangesAsync();
    }
    
    public static async Task SaveCommission(ApplicationDbContext context ,Commission commission)
    {
        var lastCom = await context.Commission
            .Where(q => q.IdCategorie == commission.IdCategorie && q.DateFin == null && q.DateDeb < DateTimeToUTC.Make((DateTime)commission.DateDeb))
            .OrderByDescending(q => q.DateDeb)
            .FirstOrDefaultAsync();
        if (lastCom != null)
        {
            lastCom.DateDeb = DateTimeToUTC.Make(lastCom.DateDeb);
            lastCom.DateFin = DateTimeToUTC.Make(commission.DateDeb);
            context.Update(lastCom);
        }
        commission.DateDeb = DateTimeToUTC.Make(commission.DateDeb);
        if (commission.DateFin != null) commission.DateFin = DateTimeToUTC.Make((DateTime)commission.DateFin);
        context.Add(commission);
        await context.SaveChangesAsync();
    }

    public static async Task SavePrixProduit(ApplicationDbContext context ,PrixProduit prixProduit)
    {
        var lastPrix = await context.PrixProduit
            .Where(q => q.IdProduit == prixProduit.IdProduit && q.DateFin == null && q.DateDeb < DateTimeToUTC.Make((DateTime)prixProduit.DateDeb))
            .OrderByDescending(q => q.DateDeb)
            .FirstOrDefaultAsync();
        if (lastPrix != null)
        {
            lastPrix.DateDeb = DateTimeToUTC.Make(lastPrix.DateDeb);
            lastPrix.DateFin = DateTimeToUTC.Make(prixProduit.DateDeb);
            context.Update(lastPrix);
        }
        prixProduit.DateDeb = DateTimeToUTC.Make(prixProduit.DateDeb);
        if (prixProduit.DateFin != null) prixProduit.DateFin = DateTimeToUTC.Make((DateTime)prixProduit.DateFin);
        context.Add(prixProduit);
        await context.SaveChangesAsync();
    }

    public async static Task<FraisImportation> LastFraisImpo(ApplicationDbContext context)
    {
        return await context.FraisImportation.OrderByDescending(q => q.DateDeb).FirstOrDefaultAsync();
    } 
    
    public async static Task<Commission> LastCommission(ApplicationDbContext context)
    {
        return await context.Commission.OrderByDescending(q => q.DateDeb).FirstOrDefaultAsync();
    }

    public static long SaveAvancer(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context ,Produit produit, double Achat, double Vente)
    {
        if (produit.IdCategorie == 0 || produit.IdType == 0 || produit.Nom.IsNullOrEmpty() ||
            produit.Description.IsNullOrEmpty() ||
            Achat == 0 || Vente == 0 )
        {
            throw new Exception("Veuillez tous remplir");
        }

        context.Add(produit);
        context.SaveChanges();
        /*Guid guid = Guid.NewGuid();
        List<string> savedFilesNames = ImageService.UploadAndResizeImages(webHostEnvironment,guid,Images,"images/produits" ,1200, 1486, false);
        ImageService.UploadAndResizeImages(webHostEnvironment,guid,Images,"images/produits" ,120, 120, true);*/
        /*int count = 0;
        
        foreach (var image in savedFilesNames)
        {
            bool isPrincipal = count == 0;
            ContenueProduit C = new ContenueProduit()
            {
                IdProduit = produit.Id, Image = image , IsPrincipal = isPrincipal
            };
            context.Add(C);
            count++;
        }*/

        PrixProduit prix = new PrixProduit()
        {
            IdProduit = produit.Id , PrixAchat = Achat , PrixVente = Vente, DateDeb = DateTimeToUTC.Make(DateTime.Now)
        };
        context.Add(prix);
        context.SaveChanges();
        return produit.Id;
    }

    public static async Task UpdateProduit(ApplicationDbContext context ,Produit produit)
    {
        if (produit.IdCategorie == 0 || produit.IdType == 0 || produit.Nom.IsNullOrEmpty() ||
            produit.Description.IsNullOrEmpty())
        {
            throw new Exception("Veuillez tous remplir");
        }

        var existingProduit = await context.Produit.FindAsync(produit.Id);

        if (existingProduit != null)
        {
            existingProduit.IdCategorie = produit.IdCategorie;
            existingProduit.IdType = produit.IdType;
            existingProduit.Nom = produit.Nom;
            existingProduit.Description = produit.Description;
            existingProduit.Fournisseur = produit.Fournisseur;
            await context.SaveChangesAsync();
        }
    }

    public static async Task ImportProduitsFromCsv(ApplicationDbContext context, IFormFile fichier)
    {
        using var reader = new StreamReader(fichier.OpenReadStream());

        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        });     

        var produits = csv.GetRecords<ProduitsCSV>().ToList();
        
        
        var categs = await context.CategorieProduit.ToListAsync();
        var genre = await context.TypeProduit.ToListAsync();
        
        
        foreach (var P in produits)
        {
            try
            {
                P.Categorie = P.Categorie.ToUpper();
                P.Genre = P.Genre.ToUpper();
                P.PourEnfant = P.PourEnfant.ToUpper();
                bool PourEnf = false;

                if (P.PourEnfant == "O" || P.PourEnfant == "TRUE" || P.PourEnfant == "T") PourEnf = true;
                else if (P.PourEnfant == "N" || P.PourEnfant == "FALSE" || P.PourEnfant == "F") PourEnf = false;
                
                CategorieProduit CAT = categs
                    .FirstOrDefault(q => q.Code.ToUpper() == P.Categorie || q.Nom.ToUpper() == P.Categorie);
                TypeProduit GENRE = genre
                    .FirstOrDefault(q => q.Nom[0].ToString().ToUpper() == P.Genre || q.Nom.ToUpper() == P.Genre); 
                if (CAT != null && GENRE!=null)
                {
                    Produit produit = new Produit()
                    {
                        IdCategorie = CAT.Id , IdType = GENRE.Id, Description = P.Description, PourEnfant = PourEnf,
                        Nom = P.Nom, Fournisseur = P.Fournisseur
                    };
                    
                    context.Add(produit);
                    await context.SaveChangesAsync();
                    
                    PrixProduit prix = new PrixProduit()
                    {
                        IdProduit = produit.Id , 
                        PrixAchat = Double.Parse(P.PrixAchat.Replace(".", ","))  , 
                        PrixVente = Double.Parse(P.PrixVente.Replace(".", ",")), 
                        DateDeb = DateTimeToUTC.Make(DateTime.Now)
                    };
                    context.Add(prix);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }

}