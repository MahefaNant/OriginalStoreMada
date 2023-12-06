using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models;

[Table("divers_depense")]
public class DiversDepense
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_typedepense")]
    public long IdTypeDepense { get; set; }
    
    [Column("corps")]
    public string Corps { get; set; }
    
    [Column("montant_ar")]
    public double MontantAr { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    /*-----------------------------------*/
    
    [ForeignKey("IdTypeDepense")]
    public virtual TypeDepense? TypeDepense { get; set; }
    
}