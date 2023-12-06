using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("entree_produit")]
public class EntreeProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_boutique")]
    public int IdBoutique { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("id_preferenceproduit")]
    public long IdPreferenceProduit { get; set; }
    
    [Column("quantiter")]
    public int Quantiter { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    
    /*-----------------------------------------*/
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique ? Boutique { get; set; }
    
    [ForeignKey("IdProduit")]
    public virtual Produit ? Produit { get; set; }
    
    [ForeignKey("IdPreferenceProduit")]
    public virtual PreferenceProduit ? PreferenceProduit { get; set; }
}