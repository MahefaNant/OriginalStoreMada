using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_diversentrer_par_ans")]
[Keyless]
public class VDiversentrerParAns
{
    [Column("annee")]
    public int Annee { get; set; }
    

    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
}