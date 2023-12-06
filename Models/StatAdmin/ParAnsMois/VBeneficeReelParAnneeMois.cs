using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_beneficereel_par_annee_mois")]
[Keyless]
public class VBeneficeReelParAnneeMois
{
    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("mois")]
    public int Mois { get; set; }
    
    [Column("recette_facture")]
    public double RecetteFacture { get; set; }
    
    [Column("recette_devis")]
    public double RecetteDevis { get; set; } 
    
    [Column("recette_divers")]
    public double RecetteDivers { get; set; }
    
    [Column("recette_total")]
    public double RecetteTotal { get; set; }
    
    [Column("depense_total")]
    public double DepenseTotal { get; set; }
    
    [Column("benefice_reel")]
    public double BeneficeReel { get; set; }
}