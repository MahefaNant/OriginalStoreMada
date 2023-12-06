using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_beneficereel_facture_par_ans_parboutique")]
[Keyless]
public class VBeneficeBeelFactureParAnsParboutique
{
    [Column("id_boutique")]
    public int IdBoutique { get; set; }
    
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
    
    /*-------------------------------------*/
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique? Boutique { get; set; }
    
}