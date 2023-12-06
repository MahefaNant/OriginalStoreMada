using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Devis;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Payements;

[Table("adresse_livraisondevis")]
public class AdresseLivraisonDevis
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_devis")]
    public long IdDevis { get; set; }
    
    [Column("id_commande")]
    public long IdCommande { get; set; }
    
    [Column("id_voldevis")]
    public long IdVolDevis { get; set; }

    [Column("ville")]
    public string Ville { get; set; }
    
    [Column("quartier")]
    public string Quartier { get; set; }
    
    [Column("longitude")]
    public double Longitude { get; set; }
    
    [Column("latitude")]
    public double Latitude { get; set; }
    
    [Column("quantiter")]
    public int Quantiter { get; set; }
    
    [Column("isinboutique")]
    public bool IsInBoutque { get; set; }
    
    [Column("fraislivraison")]
    public double FraisLivraison { get; set; }
    
    [Column("datepret")]
    public DateTime? DatePret { get; set; }
    
    [Column("dateestim")]
    public DateTime? DateEstimation { get; set; }

    [Column("date_livrer")]
    public DateTime? DateLivrer { get; set; }
    
    /*------------------------------------*/
    
    [ForeignKey("IdDevis")]
    public virtual Devis.Devis? Devis { get; set; }
    
    [ForeignKey("IdCommande")]
    public virtual CommandeDevis? CommandeDevis { get; set; }
    
    [ForeignKey("IdVolDevis")]
    public virtual VolDevis? VolDevis { get; set; }
}