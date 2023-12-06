using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.Produits.views;

[Table("v_categorieproduit")]
[Keyless]
public class VCategorieProduit
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("code")]
    public string Code { get; set; }
    
    [Column("montant_commission")]
    public double MontantCommssion { get; set; }

    [Column("montant_fraisimportation")]
    public double MontantFraisImportation { get; set; }
}