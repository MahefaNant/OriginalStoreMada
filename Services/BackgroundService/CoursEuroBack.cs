using Newtonsoft.Json.Linq;
using originalstoremada.C_;
using originalstoremada.Data;
using originalstoremada.Models.Devis;

namespace originalstoremada.Services.BackgroundService;

public class CoursEuroBack: Microsoft.Extensions.Hosting.BackgroundService
{
    
    private readonly ILogger<CoursEuroBack> _logger;
    private readonly IServiceProvider _services;
    
    public CoursEuroBack(ILogger<CoursEuroBack> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            // Spécifier l'heure exacte (13h20) avec les minutes et les secondes
            var nextExecutionTime = new DateTime(now.Year, now.Month, now.Day, 13, 3, 0);
            // Ajouter un jour si l'heure d'exécution est passée pour aujourd'hui
            if (now >= nextExecutionTime) nextExecutionTime = nextExecutionTime.AddDays(1);
            var delay = nextExecutionTime - now;
            _logger.LogInformation($"Prochaine exécution à : {nextExecutionTime}");
            // Attendre jusqu'à la prochaine exécution
            await Task.Delay(delay, stoppingToken);
            _logger.LogInformation("Exécution de la tâche périodique...");
            using (var scope = _services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                await UpdateCoursEuroAsync(dbContext, httpClientFactory);
            }
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
    
    private async Task UpdateCoursEuroAsync(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        var apiKey = "77d9c3c9e3b64f48a8dfd3f4502c358c";
        decimal euroToAriaryConversionRate = await GetConversionRate("EUR", "MGA", apiKey, httpClientFactory);

        // Calculer le montant en ariary en utilisant la conversion
        double randomMontant = 1;
        double montantAriary = Math.Round(randomMontant * (double)euroToAriaryConversionRate, 2);
        // Insérer dans la base de données
        var nouveauCours = new CoursEuro
        {
            MontantAriary = montantAriary,
            Date = DateTimeToUTC.Make(DateTime.Now)
        };
        dbContext.CoursEuro.Add(nouveauCours);
        await dbContext.SaveChangesAsync();
    }
    
    private async Task<decimal> GetConversionRate(string fromCurrency, string toCurrency, string apiKey, IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"https://openexchangerates.org/api/latest.json?app_id={apiKey}");
        HttpResponseMessage response = await client.GetAsync("");
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            var exchangeRates = JObject.Parse(responseData)["rates"];
            decimal fromRate = exchangeRates.Value<decimal>(fromCurrency);
            decimal toRate = exchangeRates.Value<decimal>(toCurrency);
    
            return toRate / fromRate;
        }
        throw new Exception("API call failed.");
    }
    
}