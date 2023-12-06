using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin.ParAnsMois.Plus;

[Table("v_diversentrer_par_ansmois_type")]
[Keyless]
public class VDiversEntrerParAnsMoisType
{
    [Column("id_typeentrer")]
    public long IdTypeEntrer { get; set; }
    
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("IdTypeEntrer")]
    public double TotalBenefice { get; set; }
    
    /*----------------------------------------*/
    
    [ForeignKey("IdTypeEntrer")]
    public virtual TypeEntrer? TypeEntrer { get; set; }
}