using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThAmCo.Products.Models;

namespace ThAmCo.Products.Services
{
    public class ExternalProductService
    {
        private readonly HttpClient _httpClient;

        public ExternalProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Fetch products from external API
        public async Task<IEnumerable<Product>> FetchProductsFromExternalApi()
        {
            string externalApiUrl = "http://undercutters.azurewebsites.net/api/Product";

            var products = await _httpClient.GetFromJsonAsync<IEnumerable<Product>>(externalApiUrl);

            return products ?? new List<Product>();
        }

        // Fetch brands from external API
        public async Task<IEnumerable<Brand>> FetchBrandsFromExternalApi()
        {
            string externalApiUrl = "http://undercutters.azurewebsites.net/api/Brand";

            var brands = await _httpClient.GetFromJsonAsync<IEnumerable<Brand>>(externalApiUrl);

            return brands ?? new List<Brand>();
        }

        // Fetch categories from external API
        public async Task<IEnumerable<Category>> FetchCategoriesFromExternalApi()
        {
            string externalApiUrl = "http://undercutters.azurewebsites.net/api/Category";

            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(externalApiUrl);

            return categories ?? new List<Category>();
        }
    }
}

