using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_diversentrer_par_ansmois")]
[Keyless]
public class VDiversentrerParAnsMois
{
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
}