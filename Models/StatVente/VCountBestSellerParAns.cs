using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.StatVente;

[Table("v_countbestseller_par_ans")]
[Keyless]
public class VCountBestSellerParAns
{

    [Column("annee")]
    public int Annee { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("total_quantite")]
    public int TotalQuantiter { get; set; }
    
    /*---------------------------------*/
    
    [ForeignKey("IdProduit")]
    public virtual Produits.Produit Produit { get; set; }
    
}