using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatVente;

[Table("v_countbestseller_par_ans_boutique")]
[Keyless]
public class VCountBestSellerParAnsBoutique : VCountBestSellerParAns
{
    [Column("id_boutique")] 
    public int IdBoutique { get; set; }
    
    /*-----------------------------------*/
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique? Boutique { get; set; }
}