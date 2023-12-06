using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatAdmin;

[Table("v_benefice_devis_reel")]
[Keyless]
public class VBeneficeDevisReel
{
  
    [Column("total_benefice")]
    public double TotalBenefice { get; set; }

}