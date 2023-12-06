using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace originalstoremada.Controllers;

public class CurrencyConversionController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    
        public CurrencyConversionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
    
        // CurrencyConversion/ConvertEuroToAriary
        public async Task<IActionResult> ConvertEuroToAriary()
        {
            decimal amount = 1;
            var apiKey = "77d9c3c9e3b64f48a8dfd3f4502c358c"; // Replace with your API key
            var euroToAriaryConversionRate = await GetConversionRate("EUR", "MGA", apiKey);
    
            decimal convertedAmount = amount * euroToAriaryConversionRate;
    
            return Content($"Converted Amount: {convertedAmount} Ariary", "text/plain");
        }
    
        public async Task<IActionResult> ConvertAriaryToEuro(decimal amount)
        {
            var apiKey = "77d9c3c9e3b64f48a8dfd3f4502c358c"; // Replace with your API key
            var ariaryToEuroConversionRate = await GetConversionRate("MGA", "EUR", apiKey);
    
            decimal convertedAmount = amount * ariaryToEuroConversionRate;
    
            return Content($"Converted Amount: {convertedAmount} Euro", "text/plain");
        }
    
        private async Task<decimal> GetConversionRate(string fromCurrency, string toCurrency, string apiKey)
        {
            var client = _httpClientFactory.CreateClient();
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
            else
            {
                throw new Exception("API call failed.");
            }
        }
}