using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using originalstoremada.Models.Boutique;

namespace originalstoremada.Models.Users;

[Table("admin")]
public class Admin
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("mail")]
    public string Mail { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("prenom")]
    public string Prenom { get; set; }
    
    [Column("adresse")] 
    public string Adresse { get; set; }
    
    [Column("niveau")]
    public int Niveau { get; set; }
    
    [Column("code")]
    public string Code { get; set; }
    
    /*-------------------------------------*/
    [NotMapped]
    public AffectationEmployer? AffectationEmployer { get; set; }
}