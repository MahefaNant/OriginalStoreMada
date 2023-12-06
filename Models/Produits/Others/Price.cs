namespace originalstoremada.Models.Produits.Others;

public class Price
{
    public double Min { get; set; }
    public double Max { get; set; }

    public Price(double min, double max)
    {
        Min = min;
        Max = max;
    }
}