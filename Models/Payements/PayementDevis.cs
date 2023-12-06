using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Payements;

[Table("payement_devis")]
public class PayementDevis
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_devis")]
    public long IdDevis { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("id_typepayement")]
    public int IdTypePayement { get;set; }

    [Column("montant")]
    public double Montant { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("datepayement")]
    public DateTime? DatePayement { get; set; }

    /*---------------------------------------*/
    
    [ForeignKey("IdDevis")]
    public virtual Devis.Devis? Devis { get; set; }
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
    
    [ForeignKey("IdTypePayement")]
    public virtual TypePayement? TypePayement { get; set; }


}