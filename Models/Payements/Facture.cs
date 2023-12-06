using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Payements;

[Table("facture")]
public class Facture
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("id_boutique")]
    public int IdBoutique { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("date_livrer")]
    public DateTime? DateLivrer { get; set; }
    
    [Column("estpayer")]
    public DateTime? EstPayer { get; set; }
    
    [Column("datesuspendue")]
    public DateTime? DateSuspendue { get; set; }
    
    /*---------------------------------*/
    
    [ForeignKey("IdClient")]
    public virtual Client ? Client {get; set; }
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique ? Boutique {get; set; }
}