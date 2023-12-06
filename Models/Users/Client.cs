using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Users;

[Table("client")]
public class Client
{
    [Key]
    [Column("id")]
    public long Id { get; set; }
    
    [Column("mail")]
    public string Mail { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("prenom")]
    public string Prenom { get; set; }
    
    [Column("adresse")]
    public string Adresse { get; set; }
    
    [Column("code")]
    public string Code { get; set; }
}