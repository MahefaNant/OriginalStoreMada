using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_benefice_devis_par_ansmois_reel")]
[Keyless]
public class VBeneficeDevisParAnsMoisReel
{
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }

}