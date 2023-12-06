using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Produits;

namespace originalstoremada.Models.Payements;

[Table("payement_produit")]
public class PayementProduit
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_facture")]
    public long IdFacture { get; set; }
    
    [Column("id_preferenceproduit")]
    public long IdPreferenceProduit { get; set; }
    
    [Column("id_typepayement")]
    public int IdTypePayement { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("datelivrer")]
    public DateTime? DateLivrer { get; set; }
    
    [Column("quantiter")]
    public int Quantiter { get; set; }
    
    [Column("montant")]
    public double Montant { get; set; }
    
    [Column("prix_achat")]
    public double Prix_achat { get; set; }

    /*--------------------------------------*/

    /*--------------------------------------*/
    
    [ForeignKey("IdFacture")]
    public virtual Facture? Facture { get; set; }
    
    [ForeignKey("IdPreferenceProduit")]
    public virtual PreferenceProduit? PreferenceProduit { get; set; }
    
    [ForeignKey("IdTypePayement")]
    public virtual TypePayement? TypePayement { get; set; }
    
    /*--------------------------------------*/
    
    public double MontantFin()
    {
        return Math.Round(Montant * Quantiter, 2);
    }
}