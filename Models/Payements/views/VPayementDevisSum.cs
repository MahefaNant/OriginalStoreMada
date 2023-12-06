using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Payements.views;

[Table("v_payementdevis_sum")]
[Keyless]
public class VPayementDevisSum
{
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
    
    [Column("total_prixreel")]
    public double? TotalPrixReel { get; set; }
    
    [Column("frais_importation")]
    public double? FraisImportation { get; set; }
    
    [Column("commission")]
    public double? Commission { get; set; }
    
    [Column("benefice")]
    public double? Benefice { get; set; }

    /*---------------------------------------*/
    
    [ForeignKey("IdDevis")]
    public virtual Devis.Devis? Devis { get; set; }
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
    
    [ForeignKey("IdTypePayement")]
    public virtual TypePayement? TypePayement { get; set; }
}