using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Payements;

[Table("type_payement")]
public class TypePayement
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("numero_resp")]
    public string NumeroResp { get; set; }
    
    [Column("nom_num")]
    public string NomNum { get; set; }
    
    [Column("nom_resp")]
    public string NomResp { get; set; }
    
}