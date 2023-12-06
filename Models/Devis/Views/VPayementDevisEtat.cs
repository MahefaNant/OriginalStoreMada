using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Devis.Views;

[Table("v_payementdevis_etat")]
[Keyless]
public class VPayementDevisEtat
{
    [Column("id_devis")]
    public long IdDevis { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("total_prixariary_reel")]
    public double TotalPrixAriaryReel { get; set; }
    
    [Column("total_prixfinariary_reel")]
    public double TotalPrixFinAriaryReel { get; set; }
    
    [Column("total_quantiter")]
    public int TotalQuantiter { get; set; }
    
    [Column("montant_payer")]
    public double MontantPayer { get; set; }
    
    [Column("pourcentage_payer")]
    public double PourcentagePayer {get; set; }
    
    [Column("ispayer")]
    public bool IsPayer {get; set; }
    
    [Column("reste_apayer")]
    public double ResteAPayer { get; set; }
    
    /*----------------------------------------------*/
    
    [ForeignKey("IdClient")]
    public Client? Client { get; set; }
    
}