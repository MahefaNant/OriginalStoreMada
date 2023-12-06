using System.Globalization;

namespace originalstoremada.C_;

public class Formattage
{
    public static string Valeur(double val)
    {
        return val.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
    }

    public static string Date(DateTime dateTime)
    {
        return dateTime.ToString("dd MMMM yyy");
    }
    
    public static string DateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd MMMM yyy HH:mm:ss");
    }
    
    public static string Time(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss");
    }

    public static string Numero(long val)
    {
        if (val < 10000) return val.ToString("0000");
        return val.ToString("00000");
    }
    
    public static string MobileMG(string numeroOriginal)
    {
        // Supprimez les espaces au début du numéro
        numeroOriginal = numeroOriginal.TrimStart();
        if (numeroOriginal.Length == 10)
        {
            string numeroFormate = numeroOriginal.Insert(3, " ").Insert(6, " ").Insert(9, " ");
            numeroOriginal = numeroFormate;
        }
        else
        {
            return numeroOriginal;
        }
        return numeroOriginal;
    }
    
    public static string InputDate(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
    }
    
    public static string InputDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
    }
}