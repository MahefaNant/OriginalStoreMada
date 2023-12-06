using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Payements;

namespace originalstoremada.Models.Devis.Views;

[Table("v_voldevis_livraison")]
[Keyless]
public class VvolDevisLivraison
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_devis")]
    public long IdDevis { get; set; }
    
    [Column("id_commande")]
    public long IdCommande { get; set; }
    
    [Column("quantiter")]
    public int Quantiter { get; set; }

    [Column("id_vol")]
    public long IdVol { get; set; }
    
    [Column("date_livrer")]
    public DateTime? DateLivrer { get; set; }
    
    [Column("ville")]
    public string? Ville { get; set; }
    
    [Column("quartier")]
    public string? Quartier { get; set; }
    
    [Column("longitude")]
    public double? Longitude { get; set; }
    
    [Column("latitude")]
    public double? Latitude { get; set; }
    
    [Column("isinboutique")]
    public bool? IsInBoutque { get; set; }
    
    [Column("fraislivraison")]
    public double? FraisLivraison { get; set; }
    
    [Column("dateestim")]
    public DateTime? DateEstim { get; set; }
    
    [Column("datepret")]
    public DateTime? DatePret { get; set; }

    [Column("in_adresse")]
    public bool InAdresse { get; set; }
    
    /*------------------------------*/

    [ForeignKey("IdCommande")]  
    public virtual CommandeDevis? CommandeDevis { get; set; }
    
    [ForeignKey("IdVol")]
    public virtual Vol? Vol { get; set; }
    
}