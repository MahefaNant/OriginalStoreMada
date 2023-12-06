using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("prix_produit")]
public class PrixProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("prix_achat")]
    public double PrixAchat { get; set; }
    
    [Column("prix_vente")]
    public double PrixVente { get; set; }
    
    [Column("datedeb")]
    public DateTime DateDeb { get; set; }
    
    [Column("datefin")]
    public DateTime? DateFin { get; set; }
    
    /*------------------------------------*/
    
    [ForeignKey("IdProduit")]
    public virtual Produit ? Produit { get; set; }
    
}