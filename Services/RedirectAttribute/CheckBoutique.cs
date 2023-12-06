using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Boutiques;

namespace originalstoremada.Services.RedirectAttribute;

public class CheckBoutique :ActionFilterAttribute
{
    private static readonly string Key = KeyStorage.KeyBoutiqueClient;
    private readonly ApplicationDbContext _context;

    public CheckBoutique(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Cookies.ContainsKey(Key) ||
            (context.HttpContext.Request.Cookies.ContainsKey(Key) &&
             context.HttpContext.Request.Cookies[Key] == "null"))
        {
            Boutique boutique = _context.Boutique.OrderByDescending(q => q.Id).First();
            context.HttpContext.Response.Cookies.Append(Key, JsonConvert.SerializeObject(boutique), CookieFunction.OptionYear(1));
            context.Result = new RedirectToActionResult("Home", "Client", null);
        }
        base.OnActionExecuting(context);
    }
}