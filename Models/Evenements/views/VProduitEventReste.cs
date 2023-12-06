using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Produits;

namespace originalstoremada.Models.Evenements.views;

[Table("v_produiteventreste")]
[Keyless]
public class VProduitEventReste
{
    [Column("id_produitevent")]
    public long Id { get; set; }
    
    [Column("id_evenement")]
    public long IdEvenement { get; set; }
    
    [Column("id_categorie")]
    public int IdCategorie { get; set; }
    
    [Column("produit_name")]
    public string Nom { get; set; }
    
    [Column("prix")]
    public double Prix { get; set; }
    
    [Column("couleur")]
    public string Couleur { get; set; }
    
    [Column("taille")]
    public string Taille { get; set; }
    
    [Column("reference_site")]
    public string? ReferenceSite { get; set; }
    
    [Column("image")]
    public string? Image { get; set; }
    
    [Column("quantiter_reste")]
    public int QuantiterReste { get; set; }
    
    [Column("quantiter_initiale")]
    public int QuantiterInitiale { get; set; }
    
    /*-------------------------------------------*/
    
    [ForeignKey("IdEvenement")]
    public virtual Evenement? Evenement { get; set; }
    
    [ForeignKey("IdCategorie")]
    public virtual CategorieProduit? CategorieProduit { get; set; }
    
    /*-------------------------------------------*/
    
    [NotMapped]
    public double? PrixAriary { get; set; }
    
    
}