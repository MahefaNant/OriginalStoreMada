using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_diversentrer")]
[Keyless]
public class VDiversentrer
{
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
}