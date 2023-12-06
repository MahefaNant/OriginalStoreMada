using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.Produits.views;

[Table("v_stockproduitglobalBoutique_details")]
[Keyless]
public class VStockProduitBoutique
{
    [Column("id_boutique")]
    public int IdBoutique { get; set; }
    
    [Column("id_produit")]
    public long IdProduit { get; set; }
    
    [Column("stock")]
    public int Stock { get; set; }
    
    [Column("longitude")]
    public double Longitude { get; set; }
    
    [Column("latitude")]
    public double Latitude { get; set; }
}