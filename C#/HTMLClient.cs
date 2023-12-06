namespace originalstoremada.C_;

public class HTMLClient
{
    public static string TitlePage(string title)
    {
        return "<section class='bg-img1 txt-center p-lr-15 p-tb-92 pal_bg1 mb-3'>"+
            "<h2 class='ltext-105 cl0 txt-center text-dark'>"+
            $"{title}"+
            "</h2>" +
            "</section>";
    }
}