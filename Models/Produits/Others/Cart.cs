namespace originalstoremada.Models.Produits.Others;

public class Cart
{
    public long IdProduit { get; set; }
    
    public long IdPref { get; set; }
    
    public string Nom { get; set; }
    
    public string Categorie { get; set; }
    
    public string Image { get; set; }
    
    public double PrixVente { get; set; }
    public double PrixAchat { get; set; }
    
    public string Couleur { get; set; }
    
    public string Taille { get; set; }
    
    public int Quantiter { get; set; }
}