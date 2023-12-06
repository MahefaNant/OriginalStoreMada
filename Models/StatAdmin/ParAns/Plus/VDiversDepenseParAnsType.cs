using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin.ParAns.Plus;

[Table("v_diversdepense_par_ans_type")]
[Keyless]
public class VDiversDepenseParAnsType
{
    [Column("id_typedepense")]
    public long IdTypeDepense { get; set; }
    
    [Column("annee")]
    public int Annee { get; set; }

    [Column("total_montant")]
    public double TotalMontant { get; set; }
    
    /*----------------------------------------*/
    
    [ForeignKey("IdTypeDepense")]
    public virtual TypeDepense? TypeDepense { get; set; }
}