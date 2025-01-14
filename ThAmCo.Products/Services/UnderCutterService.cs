using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ThAmCo.Products.Models;

namespace ThAmCo.Products.Services
{
    public class UnderCuttersService
    {
        private readonly HttpClient _httpClient;

        public UnderCuttersService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Product>> FetchProductsAsync()
        {
            string url = "http://undercutters.azurewebsites.net/api/Product";
            return await _httpClient.GetFromJsonAsync<IEnumerable<Product>>(url) ?? new List<Product>();
        }

        public async Task<IEnumerable<Category>> FetchCategoriesAsync()
        {
            string url = "http://undercutters.azurewebsites.net/api/Category";
            return await _httpClient.GetFromJsonAsync<IEnumerable<Category>>(url) ?? new List<Category>();
        }

        public async Task<IEnumerable<Brand>> FetchBrandsAsync()
        {
            string url = "http://undercutters.azurewebsites.net/api/Brand";
            return await _httpClient.GetFromJsonAsync<IEnumerable<Brand>>(url) ?? new List<Brand>();
        }

        public async Task<Order> FetchOrderByIdAsync(int id)
        {
            string url = $"http://undercutters.azurewebsites.net/api/Order/{id}";
            return await _httpClient.GetFromJsonAsync<Order>(url);
        }

        public async Task<bool> CreateOrderAsync(Order order)
        {
            string url = "http://undercutters.azurewebsites.net/api/Order";
            var response = await _httpClient.PostAsJsonAsync(url, order);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            string url = $"http://undercutters.azurewebsites.net/api/Order/{id}";
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
    }
}
