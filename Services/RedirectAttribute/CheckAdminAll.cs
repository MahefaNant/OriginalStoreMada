using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using originalstoremada.C_;
using originalstoremada.Models.Boutiques;
using originalstoremada.Models.Users;
using originalstoremada.Services.Users;

namespace originalstoremada.Services.RedirectAttribute;

public class CheckAdminAll: ActionFilterAttribute
{
    private readonly string Key = KeyStorage.KeyAdmin;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Cookies.ContainsKey(Key))
        {
            try
            {
                Admin admin = AdminService.GetByCookies(context.HttpContext.Request.Cookies[Key]);
                if (!AdminService.IsLevel_5(admin))
                {
                    JsonConvert.DeserializeObject<Boutique>(context.HttpContext.Request.Cookies[KeyStorage.KeyBoutique]);
                }
            }
            catch (Exception e)
            {
                if (context.HttpContext.Request.Cookies[Key] != null) context.HttpContext.Response.Cookies.Delete(Key);
                if (context.HttpContext.Request.Cookies[KeyStorage.KeyBoutique] != null) context.HttpContext.Response.Cookies.Delete(KeyStorage.KeyBoutique);
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
            base.OnActionExecuting(context);
        }
        else
        {
            if (context.HttpContext.Request.Cookies[Key] != null) context.HttpContext.Response.Cookies.Delete(Key);
            if (context.HttpContext.Request.Cookies[KeyStorage.KeyBoutique] != null) context.HttpContext.Response.Cookies.Delete(KeyStorage.KeyBoutique);
            context.Result = new RedirectToActionResult("Login", "Admin", null);
        }
    }
}