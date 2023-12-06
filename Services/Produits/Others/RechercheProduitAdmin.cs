namespace originalstoremada.Services.Produits.Others;

[Serializable]
public class RechercheProduitAdmin
{
    public bool All { get; set; }
    
    public string Nom { get; set; }
    public int IdCategorie { get; set; }
    public int IdType { get; set; }
    public double minPrice { get; set; }
    public double? maxPrice { get; set; }
    
    public string OrderBy { get; set; }
    public string TypeOrder { get; set; }
}