using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_benefice_devis_par_ans_reel")]
[Keyless]
public class VBeneficeDevisParAnsReel
{
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }

}