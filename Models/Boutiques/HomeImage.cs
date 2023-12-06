using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace originalstoremada.Models.Boutiques;

[Table("home_img")]
public class HomeImage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Column("title")]
    public string Title { get; set; }
    
    [Column("sub_title")]
    public string SubTitle { get; set; }
    
    [Column("image")]
    public string Image { get; set; }

}