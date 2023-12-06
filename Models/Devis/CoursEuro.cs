using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Devis;

[Table("cours_euro")]
public class CoursEuro
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("montant_ariary")]
    public double MontantAriary { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
}