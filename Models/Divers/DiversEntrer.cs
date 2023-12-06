using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models;

[Table("divers_entrer")]
public class DiversEntrer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_typeentrer")]
    public long IdTypeEntrer { get; set; }
    
    [Column("corps")]
    public string Corps { get; set; }
    
    [Column("montant_ar")]
    public double MontantAr { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    /*-----------------------------------*/
    
    [ForeignKey("IdTypeEntrer")]
    public virtual TypeEntrer? TypeEntrer { get; set; }
}