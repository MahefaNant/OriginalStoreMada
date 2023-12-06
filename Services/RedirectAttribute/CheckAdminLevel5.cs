using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using originalstoremada.C_;
using originalstoremada.Models.Users;
using originalstoremada.Services.Users;

namespace originalstoremada.Services.RedirectAttribute;

public class CheckAdminLevel5: ActionFilterAttribute
{
    private readonly string Key = KeyStorage.KeyAdmin;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Cookies.ContainsKey(Key))
        {
            try
            {
                Admin admin = AdminService.GetByCookies(context.HttpContext.Request.Cookies[Key]);
                if (AdminService.IsLevel_5(admin))
                {
                    base.OnActionExecuting(context);
                } else
                {
                    context.Result = new RedirectToActionResult("Login", "Admin", null);
                }
            }
            catch (Exception e)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
        }
        else
        {
            context.Result = new RedirectToActionResult("Login", "Admin", null);
        }
    }
}