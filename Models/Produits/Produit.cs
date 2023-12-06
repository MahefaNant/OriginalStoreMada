using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("produit")]
public class Produit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_categorie")]
    public int IdCategorie { get; set; }
    
    [Column("id_type")]
    public int IdType { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("fournisseur")]
    public string Fournisseur { get; set; }
    
    [Column("pour_enfant")]
    public bool PourEnfant { get; set; }
    
    /*----------------------------------------*/
    
    [ForeignKey("IdCategorie")]
    public virtual CategorieProduit ? CategorieProduit { get; set; }
    
    [ForeignKey("IdType")]
    public virtual TypeProduit ? TypeProduit { get; set; }
    
    [NotMapped]
    public ContenueProduit ? ImagePrincipal { get; set; }
    
    [NotMapped]
    public PrixProduit? PrixProduit { get; set; }
}