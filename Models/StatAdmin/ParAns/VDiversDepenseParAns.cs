using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;


[Table("v_diversdepense_par_ans")]
[Keyless]
public class VDiversDepenseParAns
{
    [Column("annee")]
    public int Annee { get; set; }
    
    
    [Column("total_montant")]
    public double TotalMontant { get; set; }
}