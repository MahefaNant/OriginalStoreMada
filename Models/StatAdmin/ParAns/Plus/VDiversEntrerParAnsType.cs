using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin.ParAns.Plus;

[Table("v_diversentrer_par_ans_type")]
[Keyless]
public class VDiversEntrerParAnsType
{
    [Column("id_typeentrer")]
    public long IdTypeEntrer { get; set; }
    
    [Column("annee")]
    public int Annee { get; set; }
  
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
    
    /*----------------------------------------*/
    
    [ForeignKey("IdTypeEntrer")]
    public virtual TypeEntrer? TypeEntrer { get; set; }
}