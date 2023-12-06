using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models;

[Table("v_infosmax")]
[Keyless]
public class VInfosMax
{
    [Column("max_frais_importation")]
    public double MaxFraisImpo { get; set; }
    
    [Column("max_commission")]
    public double MaxCommission { get; set; }
    
    [Column("max_cours_euro")]
    public double MaxCoursEuro { get; set; }
}