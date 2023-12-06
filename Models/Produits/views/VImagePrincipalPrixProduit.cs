using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace originalstoremada.Models.Produits.views;

[Table("v_imageprincipal_prix_produit")]
[Keyless]
public class VImagePrincipalPrixProduit
{
    [Column("id_produit")]
    public long Id { get; set; }
    
    [Column("id_categorie")]
    public int IdCategorie { get; set; }
    
    [Column("categorie")]
    public string categorie { get; set; }
    
    [Column("id_type")]
    public int IdType { get; set; }
    
    [Column("type")]
    public string Type { get; set; }
    
    [Column("pour_enfant")]
    public bool PourEnfant { get; set; }
    
    [Column("nom")]
    public string Nom { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("fournisseur")]
    public string Fournisseur { get; set; }
    
    [Column("image")]
    public string? Image { get; set; }
    
    [Column("prix_achat")]
    public double? PrixAchat { get; set; }
    
    [Column("prix_vente_initial")]
    public double? PrixVenteInitial { get; set; }
    
    [Column("date")]
    public DateTime? Date { get; set; }

    [Column("pourcentage_promotion")]
    public double? PourcentagePromotion { get; set; }
    
    [Column("promo_datedeb")]
    public DateTime? DateDebPromo { get; set; }
    
    [Column("promo_datefin")]
    public DateTime? DateFinPromo { get; set; }
    
    [Column("pourcentage_avenir")]
    public double? PourcentageAvenir { get; set; }
    
    [Column("promo_datedeb_avenir")]
    public DateTime? DateDebPromoAvenir { get; set; }
    
    [Column("promo_datefin_avenir")]
    public DateTime? DateFinPromoAvenir { get; set; }
    
    [Column("prix_vente_avec_promotion")]
    public double? PrixVenteProm { get; set; }
    
    /*----------------------------------------*/
    
    [ForeignKey("IdCategorie")]
    public virtual CategorieProduit ? CategorieProduit { get; set; }
    
    [ForeignKey("IdType")]
    public virtual TypeProduit ? TypeProduit { get; set; }
}