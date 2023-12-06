using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.Produits.views;
[Table("v_stockglobal_parpreference")]
[Keyless]
public class VStockGlobalParPreference
{
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("taille")]
    public string Taille { get; set; }
    
    [Column("id_contenue")]
    public long IdContenue { get; set; }

    [Column("id_preferenceproduit")]
    public long IdPreferenceProduit { get; set; }
    
    [Column("stock")]
    public int? Stock { get; set; }
    
    
    /*-------------------------------------*/
    
    [ForeignKey("IdContenue")]
    public virtual ContenueProduit? ContenueProduit { get; set; }
}