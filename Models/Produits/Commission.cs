using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Produits;

[Table("commission")]
public class Commission
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_categorie")]
    public int IdCategorie { get; set; }
    
    [Column("montant_euro")]
    public double MontantEuro { get; set; }
    
    [Column("datedeb")]
    public DateTime DateDeb { get; set; }
    
    [Column("datefin")]
    public DateTime? DateFin { get; set; }
    
    /*---------------------------------------*/
    
    [ForeignKey("IdCategorie")]
    public virtual CategorieProduit ? CategorieProduit { get; set; }
}