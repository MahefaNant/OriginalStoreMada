using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatVente;

[Table("v_countbestseller_par_ans_mois")]
[Keyless]
public class VCountBestSellerParAnsMois : VCountBestSellerParAns
{
    [Column("mois")]
    public int Mois { get; set; }
}