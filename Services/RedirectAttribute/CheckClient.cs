using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using originalstoremada.C_;
using originalstoremada.Models.Users;
using originalstoremada.Services.Users;

namespace originalstoremada.Services.RedirectAttribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class CheckClient: ActionFilterAttribute
{
    private readonly string Key = KeyStorage.KeyClient;
    public string RedirectController { get; set; }
    public string RedirectAction { get; set; }
    public string ErrorMessage { get; set; }
    // public ITempDataDictionary TempData { get; set; }
    
    public CheckClient(string redirectController = "Client", string redirectAction = "Home", string errorMessage = "Vous devez vous connecter pour accéder à cette page.")
    {
        RedirectController = redirectController;
        RedirectAction = redirectAction;
        ErrorMessage = errorMessage;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        /*ITempDataProvider tempDataProvider = context.HttpContext.RequestServices.GetRequiredService<ITempDataProvider>();
        IDictionary<string, object> tempData = tempDataProvider.LoadTempData(context.HttpContext);*/
        if (context.HttpContext.Request.Cookies.ContainsKey(Key))
        {
            try
            {
                Client client = ClientService.GetByCookies(context.HttpContext.Request.Cookies[Key]);
            }
            catch (Exception e)
            {
                /*tempData["ErrorMessage"] = ErrorMessage;
                tempDataProvider.SaveTempData(context.HttpContext, tempData);*/
                var model = new { RedirectController, RedirectAction };
                context.Result = new RedirectToActionResult( "Login", "Client", model);
            }
            base.OnActionExecuting(context);
        }
        else
        {
            /*tempData["RedirectController"] = RedirectController;
            tempDataProvider.SaveTempData(context.HttpContext, tempData);*/
            var model = new { RedirectController, RedirectAction };
            context.Result = new RedirectToActionResult( "Login", "Client", model);
        }
    }
}