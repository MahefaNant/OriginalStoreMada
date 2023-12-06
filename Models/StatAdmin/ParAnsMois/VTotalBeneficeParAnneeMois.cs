using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;


[Table("v_total_benefice_par_annee_mois")]
[Keyless]
public class VTotalBeneficeParAnneeMois
{
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("benefice_facture")]
    public double BeneficeFacture { get; set; }
    
    [Column("benefice_devis")]
    public double BeneficeDevis { get; set; } 
    
    [Column("benefice_divers")]
    public double BeneficeDivers { get; set; }
    
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }
}