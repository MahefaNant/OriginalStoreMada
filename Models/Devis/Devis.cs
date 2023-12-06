using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Devis;

[Table("devis")]
public class Devis
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("cours_devis")]
    public double? CoursDevis { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("date_envoi")]
    public DateTime? DateEnvoi { get; set; }
    
    [Column("date_validation")]
    public DateTime? DateValidation { get; set; }
    
    [Column("date_delete")]
    public DateTime? DateDelete { get; set; }
    
    [Column("date_payer")]
    public DateTime? DatePayer { get; set; }
    
    [Column("remarque")]
    public string? Remarque { get; set; }
    
    /*----------------------------------*/
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
}