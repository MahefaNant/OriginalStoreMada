using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using originalstoremada.Models;
using originalstoremada.Models.Boutique;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Devis;
using originalstoremada.Models.Devis.Views;
using originalstoremada.Models.Evenements;
using originalstoremada.Models.Evenements.views;
using originalstoremada.Models.Payements;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.Produits;
using originalstoremada.Models.Produits.views;
using originalstoremada.Models.StatAdmin;
using originalstoremada.Models.StatAdmin.ParAns.Plus;
using originalstoremada.Models.StatAdmin.ParAnsMois.Plus;
using originalstoremada.Models.StatVente;
using originalstoremada.Models.Users;

namespace originalstoremada.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Client> Client { get; set; } = default!;
    public DbSet<Admin> Admin { get; set; } = default!;
    public DbSet<Vol> Vol { get; set; } = default!;
    public DbSet<Boutique> Boutique { get; set; } = default!;
    public DbSet<AffectationEmployer> AffectationEmployer { get; set; } = default!;
    public DbSet<CategorieProduit> CategorieProduit { get; set; } = default!;
    public DbSet<FraisImportation> FraisImportation { get; set; } = default!;
    public DbSet<Commission> Commission { get; set; } = default!;
    public DbSet<TypeProduit> TypeProduit { get; set; } = default!;
    public DbSet<Produit> Produit { get; set; } = default!;
    public DbSet<ContenueProduit> ContenueProduit { get; set; } = default!;
    public DbSet<PrixProduit> PrixProduit { get; set; } = default!;
    public DbSet<VImagePrincipalPrixProduit> VImagePrincipalPrixProduit { get; set; } = default!;
    public DbSet<PreferenceProduit> PreferenceProduit { get; set; } = default!;
    public DbSet<EntreeProduit> EntreeProduit { get; set; } = default!;
    
    public DbSet<SortieProduit> SortieProduit { get; set; } = default!;
    public DbSet<Devis> Devis { get; set; } = default!;
    public DbSet<CommandeDevis> CommandeDevis { get; set; } = default!;
    public DbSet<VDevisInfo> VDevisInfos { get; set; } = default!;
    public DbSet<VStockProduitGlobal> VStockProduitGlobal { get; set; } = default!;
    public DbSet<VStockProduit> VStockProduit { get; set; } = default!;
    public DbSet<VCommanceDevis> VCommanceDevis { get; set; } = default!;
    public DbSet<CoursEuro> CoursEuro { get; set; } = default!;
    public DbSet<VInfosMax> VInfosMax { get; set; } = default!;
    public DbSet<PayementDevis> PayementDevis { get; set; } = default!;
    public DbSet<VPayementDevis> VPayementDevis { get; set; } = default!;
    public DbSet<VPayementDevisSum> VPayementDevisSum { get; set; } = default!;
    public DbSet<VPayementDevisEtat> VPayementDevisEtat { get; set; } = default!;
    public DbSet<TypePayement> TypePayement { get; set; } = default!;
    public DbSet<VolDevis> VolDevis { get; set; } = default!;
    public DbSet<VStockProduitBoutique> VStockProduitBoutique { get; set; } = default!;
    public DbSet<VCartProduit> VCartProduit { get; set; } = default!;
    public DbSet<Facture> Facture { get; set; } = default!;
    public DbSet<PayementProduit> PayementProduit { get; set; } = default!;
    public DbSet<AdresseLivraisonFacture> AdresseLivraisonFacture { get; set; } = default!;
    public DbSet<VFacture> VFacture { get; set; } = default!;
    public DbSet<VPayementProduit> VPayementProduit { get; set; } = default!;
    public DbSet<AdresseLivraisonDevis> AdresseLivraisonDevis { get; set; } = default!;
    public DbSet<VStockPreference> VStockPreference { get; set; } = default!;
    public DbSet<VCategorieProduit> VCategorieProduit { get; set; } = default!;
    public DbSet<Evenement> Evenement { get; set; } = default!;
    public DbSet<ProduitEvent> ProduitEvent { get; set; } = default!;
    public DbSet<InteractionEvent> InteractionEvent { get; set; } = default!;
    public DbSet<VProduitEventReste> VProduitEventReste { get; set; } = default!;
    public DbSet<HomeImage> HomeImage { get; set; } = default!;
    
    public DbSet<VBeneficeDevisParAnsMoisReel> VBeneficeDevisParAnsMoisReel { get; set; } = default!;
    public DbSet<VBeneficeReelFactureParAnsMois> VBeneficeReelFactureParAnsMois { get; set; } = default!;
    public DbSet<VBeneficeBeelFactureParAnsMoisParboutique> VBeneficeBeelFactureParAnsMoisParboutique { get; set; } = default!;
    public DbSet<VTotalBeneficeParAnneeMois> VTotalBeneficeParAnneeMois { get; set; } = default!;
    public DbSet<VDiversentrerParAnsMois> VDiversentrerParAnsMois { get; set; } = default!;
    public DbSet<VDiversDepenseParAnsMois> VDiversDepenseParAnsMois { get; set; } = default!;
    
    public DbSet<VBeneficeDevisParAnsReel> VBeneficeDevisParAnsReel { get; set; } = default!;
    public DbSet<VBeneficeReelFactureParAns> VBeneficeReelFactureParAns { get; set; } = default!;
    public DbSet<VBeneficeBeelFactureParAnsParboutique> VBeneficeBeelFactureParAnsParboutique { get; set; } = default!;
    public DbSet<VTotalBeneficeParAnnee> VTotalBeneficeParAnnee { get; set; } = default!;
    public DbSet<VDiversentrerParAns> VDiversentrerParAns { get; set; } = default!;
    public DbSet<VDiversDepenseParAns> VDiversDepenseParAns { get; set; } = default!;
    
    public DbSet<VBeneficeDevisReel> VBeneficeDevisReel { get; set; } = default!;
    public DbSet<VBeneficeReelFacture> VBeneficeReelFacture { get; set; } = default!;
    public DbSet<VBeneficeBeelFactureParboutique> VBeneficeBeelFactureParboutique { get; set; } = default!;
    public DbSet<VTotalBenefice> VTotalBenefice { get; set; } = default!;
    public DbSet<VDiversentrer> VDiversentrer { get; set; } = default!;
    public DbSet<VDiversDepense> VDiversDepense { get; set; } = default!;
    
    public DbSet<VBeneficeReelParAnneeMois> VBeneficeReelParAnneeMois { get; set; } = default!;
    public DbSet<VBeneficeReelParAnnee> VBeneficeReelParAnnee { get; set; } = default!;
    public DbSet<VBeneficeReel> VBeneficeReel { get; set; } = default!;
    public DbSet<FavorisProduit> FavorisProduit { get; set; } = default!;
    public DbSet<TypeDepense> TypeDepense { get; set; } = default!;
    public DbSet<TypeEntrer> TypeEntrer { get; set; } = default!;
    public DbSet<DiversEntrer> DiversEntrer { get; set; } = default!;
    public DbSet<DiversDepense> DiversDepense { get; set; } = default!;
    public DbSet<VDiversDepenseParAnsMoisType> VDiversDepenseParAnsMoisType { get; set; } = default!;
    public DbSet<VDiversEntrerParAnsMoisType> VDiversEntrerParAnsMoisType { get; set; } = default!;
    public DbSet<VDiversEntrerParAnsType> VDiversEntrerParAnsType { get; set; } = default!;
    public DbSet<VDiversDepenseParAnsType> VDiversDepenseParAnsType { get; set; } = default!;
    public DbSet<VCommandeDevisResteInVol> VCommandeDevisResteInVol { get; set; } = default!;
    public DbSet<VvolDevisLivraison> VvolDevisLivraison { get; set; } = default!;
    public DbSet<VStockGlobalParPreference> VStockGlobalParPreference { get; set; } = default!;
    public DbSet<PromotionProduit> PromotionProduit { get; set; } = default!;
    public DbSet<VCountBestSeller> VCountBestSeller { get; set; } = default!;
    public DbSet<VCountBestSellerParBoutique> VCountBestSellerParBoutique { get; set; } = default!;
    public DbSet<VCountBestSellerParAns> VCountBestSellerParAns { get; set; } = default!;
    public DbSet<VCountBestSellerParAnsMois> VCountBestSellerParAnsMois { get; set; } = default!;
    public DbSet<VCountBestSellerParAnsBoutique> VCountBestSellerParAnsBoutique { get; set; } = default!;
    public DbSet<VCountBestSellerParAnsMoisBoutique> VCountBestSellerParAnsMoisBoutique { get; set; } = default!;


}