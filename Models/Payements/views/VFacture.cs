using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Payements.views;

[Table("v_facture")]
[Keyless]
public class VFacture
{
    [Column("id_facture")]
    public long Id { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("id_boutique")]
    public int IdBoutique { get; set; }
    
    [Column("id_typepayement")]
    public int IdTypePayement { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("date_livrer")]
    public DateTime? DateLivrer { get; set; }
    
    [Column("quantiter_total")]
    public double QuantiterTotal { get; set; }
    
    [Column("montant_sum")]
    public double MontantSum { get; set; }
    
    [Column("montant_fin")]
    public double MontantFin { get; set; }
    
    [Column("ville_adresse")]
    public string VilleAdresse { get; set; }
    
    [Column("quartier_adresse")]
    public string QuartierAdresse { get; set; }
    
    [Column("longitude_adresse")]
    public double longitudeAdresse { get; set; }
    
    [Column("latitude_adresse")]
    public double LatitudeAresse { get; set; }
    
    [Column("isinboutique")]
    public bool IsInBoutique { get; set; } 
    
    [Column("fraislivraison")]
    public double FraisLivraison { get; set; }
    
    [Column("datepret")]
    public DateTime? DatePret { get; set; }
    
    [Column("dateestim")]
    public DateTime? DateEstim { get; set; }
    
    [Column("estpayer")]
    public DateTime? EstPayer { get; set; }
    
    [Column("datesuspendue")]
    public DateTime? DateSuspendue { get; set; }
    
    /*---------------------------------------*/
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique? Boutique { get; set; }
    
    [ForeignKey("IdTypePayement")]
    public virtual TypePayement? TypePayement { get; set; }
    
    /*-------------------------------------------*/
    
    [NotMapped]
    public string EtatLivraisonType { get; set; }
    
    [NotMapped]
    public string EtatLivraison { get; set; }
    
    
}