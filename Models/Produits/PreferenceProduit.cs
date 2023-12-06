using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("preference_produit")]
public class PreferenceProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("taille")]
    public string Taille { get; set; }
    
    [Column("id_contenue")]
    public long IdContenue { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }

    /*----------------------------------------*/
    
    [ForeignKey("IdProduit")]
    public virtual Produit ? Produit { get; set; }
    
    [ForeignKey("IdContenue")]
    public virtual ContenueProduit ? ContenueProduit { get; set; }
}