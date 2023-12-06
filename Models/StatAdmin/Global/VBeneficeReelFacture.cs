using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_beneficereel_facture")]
[Keyless]
public class VBeneficeReelFacture
{
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
}