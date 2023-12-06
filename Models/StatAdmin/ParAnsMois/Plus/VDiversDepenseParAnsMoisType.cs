using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin.ParAnsMois.Plus;

[Table("v_diversdepense_par_ansmois_type")]
[Keyless]
public class VDiversDepenseParAnsMoisType
{
    [Column("id_typedepense")]
    public long IdTypeDepense { get; set; }
    
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("total_montant")]
    public double TotalMontant { get; set; }
    
    /*----------------------------------------*/
    
    [ForeignKey("IdTypeDepense")]
    public virtual TypeDepense? TypeDepense { get; set; }
    
}