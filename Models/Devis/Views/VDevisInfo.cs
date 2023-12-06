using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Devis.Others;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Devis.Views;

[Table("v_devisinfo")]
[Keyless]
public class VDevisInfo
{
    [Column("id_devis")]
    public long Id { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("total_prixeuro")]
    public double TotalPrixEuro { get; set; }
    
    [Column("total_prixfin_euro")]
    public double TotalPrixFinEuro { get; set; }
    
    [Column("total_prixariary_reel")]
    public double TotalPrixAriaryReel { get; set; }
    
    [Column("total_prixfinariary_reel")]
    public double TotalPrixFinAriaryReel { get; set; }
    
    [Column("total_prixariary_1")]
    public double TotalPrixAriary1 { get; set; }
    
    [Column("total_prixfinariary_1")]
    public double TotalPrixFinAriary1 { get; set; }
    
    [Column("total_quantiter")]
    public int TotalQuantiter { get; set; }
    
    [Column("total_frais_importation")]
    public double TotalFraisImportation { get; set; }
    
    [Column("total_frais_importation_ariary")]
    public double TotalFraisImportationAriary { get; set; }
    
    [Column("total_commission")]
    public double TotalCommission { get; set; }
    
    [Column("total_commission_ariary")]
    public double TotalCommissionAriary { get; set; }
    
    [Column("total_frais_importation_reel")]
    public double? TotalFraisImportationReel { get; set; }
    
    [Column("total_frais_importation_reelariary")]
    public double? TotalFraisImportationReelAriary { get; set; }
    
    [Column("total_commission_reel")]
    public double? TotalCommissionReel { get; set; }
    
    [Column("total_commission_reelariary")]
    public double? TotalCommissionReelAriary { get; set; }
    
    [Column("cours_devis")]
    public double? CoursDevis { get; set; }

    [Column("date_envoi")]
    public DateTime? DateEnvoi { get; set; }
    
    [Column("date_validation")]
    public DateTime? DateValidation { get; set; }
    
    [Column("date_delete")]
    public DateTime? DateDelete { get; set; }
    
    [Column("is_livrer")]
    public bool IsLivrer { get; set; }
    
    [Column("date_payer")]
    public DateTime? DatePayer { get; set; }
    
    [Column("cours_euro")]
    public double? CoursEuro { get; set; }
    
    [Column("remarque")]
    public string? Remarque { get; set; }
    
    [Column("etatpayement")]
    public bool EtatPayement { get; set; }
    
    /*-----------------------------*/
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
    
    /*------------------------------*/
    
    [NotMapped]
    public EtatDevis EtatActuel { get; set; }
    
    [NotMapped]
    public double ActuelEuroElement { get; set; }
    
    [NotMapped]
    public double ActuelEuro { get; set; }
    
    
    [NotMapped]
    public double ActuelAriaryElement { get; set; }
    
    [NotMapped]
    public double ActuelAriary { get; set; }
    
}