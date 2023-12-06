using Microsoft.AspNetCore.Mvc;

namespace originalstoremada.Controllers;

public class ProductController : Controller
{
    
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    // GET
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://dummyjson.com/");

        HttpResponseMessage response = await client.GetAsync("products");
        if (response.IsSuccessStatusCode)
        {
            var productData = await response.Content.ReadAsStringAsync();
            Console.WriteLine(productData);
            /*var products = JsonConvert.DeserializeObject<List<dynamic>>(productData);

            if (products != null && products.Count > 0)
            {
                return Ok();
            }
            else
            {
                return Content("No products found.");
            }*/
        }
        else
        {
            return Ok();
        }

        return Ok();
    }
}