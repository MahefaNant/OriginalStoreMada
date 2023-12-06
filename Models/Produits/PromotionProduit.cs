using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("promotion_produit")]
public class PromotionProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("pourcentage")]
    public double Pourcenttage { get; set; }
    
    [Column("datedeb")]
    public DateTime DateDeb { get; set; }
    
    [Column("datefin")]
    public DateTime DateFin { get; set; }
    
    /*------------------------------------*/
    
    [ForeignKey("IdProduit")]
    public virtual Produit ? Produit { get; set; }
}