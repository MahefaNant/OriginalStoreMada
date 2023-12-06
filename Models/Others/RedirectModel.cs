namespace originalstoremada.Models.Others;

public class RedirectModel
{
    public string asp_controller { get; set; } 
    public string asp_action { get; set; } 

    
    public RedirectModel(string asp_controller, string asp_action)
    {
        this.asp_controller = asp_controller;
        this.asp_action = asp_action;
    }

}