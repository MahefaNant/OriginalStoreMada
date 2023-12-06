using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using originalstoremada.Models.Produits;

namespace originalstoremada.Models.Devis.Views;

[Table("v_commandedevis")]
[Keyless]
public class VCommanceDevis
{
    [Column("id")]
    public long Id { get; set; }
    
    [Column("id_devis")]
    public long IdDevis { get; set; }
    
    [Column("id_categorie")]
    public int IdCategorie { get; set; }
    
    [Column("produit_name")]
    public string ProduitName { get; set; }
    
    [Column("prix_euro")]
    public double PrixEuro { get; set; }
    
    [Column("couleur")]
    public string Couleur { get; set; }
    
    [Column("taille")]
    public string Taille { get; set; }
    
    [Column("nombre")]
    public int Nombre { get; set; }
    
    [Column("reference_site")]
    public string ReferenceSite { get; set; }
    
    [Column("image")]
    public string? image { get; set; }
    
    [Column("frais_importation_montant")]
    public double FraisImpoMontant { get; set; }
    
    [Column("commission_montant")]
    public double CommissionMontant { get; set; }
    
    [Column("frais_importation_reel")]
    public double? FraisImporReel { get; set; }
    
    [Column("commission_reel")]
    public double? CommissionReel { get; set; }
    
    /*----------------------------*/
    
    [ForeignKey("IdDevis")]
    public virtual Devis? Devis { get; set; }
    
    [ForeignKey("IdCategorie")]
    public virtual CategorieProduit? CategorieProduit { get; set; }
    
}