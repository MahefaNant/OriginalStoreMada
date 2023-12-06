using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Produits;

[Table("favoris_produit")]
public class FavorisProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    /*---------------------------------------*/
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
    
    [ForeignKey("IdProduit")]
    public virtual Produit? Produit { get; set; }
    
}