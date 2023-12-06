using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("categorie_produit")]
public class CategorieProduit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("code")]
    public string Code { get; set; }
    
    /*--------------------------------------*/
    
    [NotMapped]
    public FraisImportation? FraisImportation { get; set; }
    
    [NotMapped]
    public Commission? Commission { get; set; }
}