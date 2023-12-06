using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatVente;

[Table("v_countbestseller_par_ans_mois_boutique")]
[Keyless]
public class VCountBestSellerParAnsMoisBoutique : VCountBestSellerParAnsMois
{
    [Column("id_boutique")] 
    public int IdBoutique { get; set; }
    
    /*-----------------------------------*/
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique? Boutique { get; set; }
}