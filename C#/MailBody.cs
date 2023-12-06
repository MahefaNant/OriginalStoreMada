using originalstoremada.Models.Payements;
using originalstoremada.Models.Payements.views;
using originalstoremada.Models.Users;
using originalstoremada.Services.Mail;

namespace originalstoremada.C_;

public class MailBody
{
    private static string MainMail = "mahefanant@gmail.com";

    public static void ProduitFactureAfterPret( SmtpConfig smtpConfig ,string mailClient, long id_facture, VFacture facture)
    {
        string sujet = "";
        string body = @"<h1>Original Store Mada</h1>";
        if (facture.IsInBoutique)
        {
            sujet = "<h2>Information de Recuperation de vos produits</h2>";
            body += $@"
                <h3>Facture Num_{id_facture}</h3>
                <p>Vos produits sont prêts à être récupérés!</p>
                <p>Vous pouvez les récupérer dès maintenant.</p>
                <p>Boutique de récupération:</p>
                <ul>
                    <li>Ville : {facture.VilleAdresse}</li>
                    <li>Quartier : {facture.QuartierAdresse}</li>
                    <li>Precision Longitude : {facture.longitudeAdresse}</li>
                    <li>Precision Latitude : {facture.LatitudeAresse}</li>
                </ul>
                <p>Veuillez bien retenir le numéro de votre facture.</p>
            ";
        }
        else
        {
            sujet = "<h2>Information de Livraison de vos produits</h2>";
            body += $@"
                <h1>Facture Num_{id_facture}</h1>
                <p>Vos produits sont prêts à être livrés!</p>
                <p>Information de livraison:</p>
                <ul>
                    <li>Ville : {facture.VilleAdresse}</li>
                    <li>Quartier : {facture.QuartierAdresse}</li>
                    <li>Precision Longitude : {facture.longitudeAdresse}</li>
                    <li>Precision Latitude : {facture.LatitudeAresse}</li>
                    <li>Estimation du date de Livraison : {Formattage.DateTime((DateTime)facture.DateEstim)}</li>
                </ul>
                <p>Veuillez bien retenir le numéro de votre facture.</p>
            ";
        }

        MailService.EnvoyerEmail(smtpConfig,MainMail, mailClient,sujet, body);
    }

    public static void MessageFinalisationLivraison(SmtpConfig smtpConfig, Facture facture, Client client)
    {
        string sujet = "Finalisation Livraison ou récupération des éléments de la facture";
        string body = "Original Store Mada \n";
        body += $"Livraison ou récupération des éléments de la facture numero {facture.Id} avec succés";
        
        MailService.EnvoyerEmail(smtpConfig,MainMail, client.Mail,sujet, body);
    }
}