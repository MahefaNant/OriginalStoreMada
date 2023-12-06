using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatVente;

[Table("v_countbestseller_parboutique")]
[Keyless]
public class VCountBestSellerParBoutique: VCountBestSeller
{
    [Column("id_boutique")] 
    public int IdBoutique { get; set; }
    
    /*-----------------------------------*/
    
    [ForeignKey("IdBoutique")]
    public virtual Boutiques.Boutique? Boutique { get; set; }
}