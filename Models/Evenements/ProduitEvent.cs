using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Produits;

namespace originalstoremada.Models.Evenements;

[Table("produit_event")]
public class ProduitEvent
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_evenement")]
    public long IdEvenement { get; set; }
    
    [Column("id_categorie")]
    public int IdCategorie { get; set; }
    
    [Column("produit_name")]
    public string ProduitName { get; set; }
    
    [Column("prix")]
    public double Prix { get; set; }
    
    [Column("couleur")]
    public string Couleur { get; set; }
    
    [Column("taille")]
    public string Taille { get; set; }
    
    [Column("quantitermax")]
    public int QuantiterMax { get; set; }
    
    [Column("reference_site")]
    public string? ReferenceSite { get; set; }

    [Column("image")]
    public string? Image { get; set; }
    
    /*-------------------------------------------------*/
    [ForeignKey("IdEvenement")]
    public virtual Evenement? Evenement { get; set; }
    
    [ForeignKey("IdCategorie")]
    public virtual CategorieProduit? CategorieProduit { get; set; }
}