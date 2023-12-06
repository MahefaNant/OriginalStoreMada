using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Boutique;

[Table("affectation_employer")]
public class AffectationEmployer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("id_admin")]
    public int IdAdmin { get; set; }
    
    [Column("id_boutique")]
    public int IdBoutique { get; set; }
    
    [Column("datedeb")]
    public DateTime DateDeb { get; set; }
    
    [Column("datefin")]
    public DateTime? DateFin { get; set; }
    
    /*-----------------------------------*/
    
    [ForeignKey("IdAdmin")]
    public virtual Admin? Admin { get; set; }
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique? Boutique { get; set; }
}