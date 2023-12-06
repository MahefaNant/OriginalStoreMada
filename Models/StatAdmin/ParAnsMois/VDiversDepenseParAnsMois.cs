using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;


[Table("v_diversdepense_par_ansmois")]
[Keyless]
public class VDiversDepenseParAnsMois
{
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("total_montant")]
    public double TotalMontant { get; set; }
}