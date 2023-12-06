using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("contenue_produit")]
public class ContenueProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("couleur")]
    public string Couleur { get; set; }
    
    [Column("image")]
    public string Image { get; set; }
    
    [Column("isprincipal")]
    public bool IsPrincipal { get; set; }
    
    /*--------------------------------*/
    
    /*------------------------------------*/
    
    [ForeignKey("IdProduit")]
    public virtual Produit ? Produit { get; set; }
}