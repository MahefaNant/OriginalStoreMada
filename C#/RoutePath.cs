namespace originalstoremada.C_;

public class RoutePath
{

    public static string HtmlTemplateAdmin()
    {
        return "~/Views/Admin/templates/_HtmlTemplate.cshtml";
    }
    
    public static string NavAdmin()
    {
        return "~/Views/Admin/templates/_Nav.cshtml";
    }
    
    public static string AsideAdmin()
    {
        return "~/Views/Admin/templates/_Aside.cshtml";
    }

    public static string ContentAdmin()
    {
        return "~/Views/Admin/templates/_Content.cshtml";
    }
    
    /*-------------------------------*/
    
    public static string HtmlTemplateClient()
    {
        return "~/Views/Client/templates/_HtmlTemplate.cshtml";
    }
    
    public static string NavClient()
    {
        return "~/Views/Client/templates/_Nav.cshtml";
    }
    
    public static string FooterClient()
    {
        return "~/Views/Client/templates/_Footer.cshtml";
    }

    public static string ContentClient()
    {
        return "~/Views/Client/templates/_Content.cshtml";
    }
    
    public static string BoutiqueChooseClient()
    {
        return "~/Views/Client/templates/_BoutiqueChoose.cshtml";
    }
}