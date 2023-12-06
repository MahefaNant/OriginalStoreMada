using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Devis;

[Table("vol")]
public class Vol
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
   
    [Column("datedepart")]
    public DateTime DateDepart { get; set; }
    
    [Column("datearriver_estimmer")]
    public DateTime DateArriverEstimer { get; set; }
    
    [Column("datearriver")]
    public DateTime? DateArriver{ get; set; }
}