using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Boutiques;

[Table("boutique")]
public class Boutique
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("ville")]
    public string Ville { get; set; }
    
    [Column("quartier")]
    public string Quartier { get; set; }
    
    [Column("adresse")]
    public string adresse { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("longitude")]
    public double Longitude { get; set; }
    
    [Column("latitude")]
    public double Latitude { get; set; }
    
    /*-------------------------------------*/
    
    [NotMapped]
    public double? DistParApport { get; set; }
}