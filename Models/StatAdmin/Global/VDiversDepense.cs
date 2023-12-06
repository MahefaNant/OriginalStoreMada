using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;


[Table("v_diversdepense")]
[Keyless]
public class VDiversDepense
{

    [Column("total_montant")]
    public double TotalMontant { get; set; }
}