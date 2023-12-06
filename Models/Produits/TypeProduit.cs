using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("type_produit")]
public class TypeProduit
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    /*--------------------------*/

    [NotMapped] public bool IsCheked = false;
}