using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Payements;

[Table("adresse_livraisonfacture")]
public class AdresseLivraisonFacture
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_facture")]
    public long IdFacture { get; set; }
    
    [Column("ville")]
    public string Ville { get; set; }
    
    [Column("quartier")]
    public string Quartier { get; set; }
    
    [Column("longitude")]
    public double Longitude { get; set; }
    
    [Column("latitude")]
    public double Latitude { get; set; }
    
    [Column("isinboutique")]
    public bool IsInBoutque { get; set; }
    
    [Column("fraislivraison")]
    public double FraisLivraison { get; set; }
    
    [Column("datepret")]
    public DateTime? DatePret { get; set; }
    
    [Column("dateestim")]
    public DateTime? DateEstimation { get; set; }
    
    /*------------------------------------------*/
    
    [ForeignKey("IdFacture")]
    public virtual Facture ? Facture { get; set; }
    
}