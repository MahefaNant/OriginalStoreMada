using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Devis;

[Table("vol_devis")]
public class VolDevis
{
    [Key]
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
    
    /*------------------------------*/

    [ForeignKey("IdCommande")]  
    public virtual CommandeDevis? CommandeDevis { get; set; }
    
    [ForeignKey("IdVol")]
    public virtual Vol? Vol { get; set; }
}