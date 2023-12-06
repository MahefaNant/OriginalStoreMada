using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Users;

namespace originalstoremada.Models.Evenements;

[Table("interaction_event")]
public class InteractionEvent
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_evenement")]
    public long IdEvenement { get; set; }
    
    [Column("id_produitevent")]
    public long IdProduitEvent { get; set; }
    
    [Column("id_client")]
    public long IdClient { get; set; }
    
    [Column("quantiter")]
    public int Quantiter { get; set; }
    
    /*--------------------------------------*/
    
    [ForeignKey("IdEvenement")]
    public virtual Evenement? Evenement { get; set; }
    
    [ForeignKey("IdProduitEvent")]
    public virtual ProduitEvent? ProduitEvent { get; set; }
    
    [ForeignKey("IdClient")]
    public virtual Client? Client { get; set; }
}