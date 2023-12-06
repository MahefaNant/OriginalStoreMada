using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatVente;

[Table("v_countbestseller")]
[Keyless]
public class VCountBestSeller
{
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("nombre_seller")]
    public int TotalQuantiter { get; set; }
    
    /*---------------------------------*/
    
    [ForeignKey("IdProduit")]
    public virtual Produits.Produit Produit { get; set; }
}