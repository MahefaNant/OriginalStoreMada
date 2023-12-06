using Microsoft.AspNetCore.Mvc;
using originalstoremada.C_;
using originalstoremada.Models.Produits;
using originalstoremada.Services.Mail;

namespace originalstoremada.Controllers;

public class TestController : Controller
{
    // GET

    private readonly SmtpConfig _smtpConfig;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public TestController(SmtpConfig smtpConfig, IWebHostEnvironment webHostEnvironment)
    {
        _smtpConfig = smtpConfig;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        string corps = $@"
                <html>
                <head>
                    <title>Original Store Mada</title>
                </head>
                <body>
                    <h1>Facture Num_5</h1>
                    <p>Vos produits sont prêts à être récupérés!</p>
                    <p>Vous pouvez les récupérer dès maintenant.</p>
                    <p>Boutique de récupération:</p>
                    <ul>
                        <li>Ville : add</li>
                        <li>Quartier : qwe</li>
                        <li>Precision Longitude : sad</li>
                        <li>Precision Latitude : adas</li>
                    </ul>
                    <p>Veuillez bien retenir le numéro de votre facture.</p>
                </body>
                </html>
            ";
        MailService.EnvoyerEmail(_smtpConfig,"mahefanant@gmail.com", "mahefanant@gmail.com","TEST MAHEFA", corps);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upload(List<IFormFile> Images)
    {
        try
        {
            Guid guid = Guid.NewGuid();
            List<string> savedFilesNames = ImageService.UploadAndResizeImages(_webHostEnvironment,guid,Images,"images/OSM" ,1024, 1024, false);
            ImageService.UploadAndResizeImages(_webHostEnvironment,guid,Images,"images/OSM" ,100, 100, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return Ok();
    }

    public IActionResult CopyFile()
    {
        // ImageService.CopyFile(_webHostEnvironment,"images/events/974c4dd6-5f4f-4565-8348-07d41758dd79_chaussures-led-lumineuses.jpg","images");
        return Ok();
    }
}