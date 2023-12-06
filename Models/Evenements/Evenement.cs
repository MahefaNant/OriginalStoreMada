using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Evenements.views;

namespace originalstoremada.Models.Evenements;

[Table("evenement")]
public class Evenement
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("datedeb")]
    public DateTime DateDeb { get; set; }
    
    [Column("datefin")]
    public DateTime DateFin { get; set; }
    
    /*----------------------------------------*/
    
    [NotMapped]
    public List<VProduitEventReste>? VProduitEventReste { get; set; }
}