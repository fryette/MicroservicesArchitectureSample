using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using ShoppingCart.ShoppingCart;
using ShoppingCart.ShoppingCart.Models;
using CacheControlHeaderValue = Microsoft.Net.Http.Headers.CacheControlHeaderValue;

namespace ShoppingCart
{
    public class ProductCatalogueClient : IProductCatalogueClient
    {
        private static readonly Policy ExponentialRetryPolicy =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                    (ex, _) => Console.WriteLine(ex.ToString()));

        private static readonly string productCatalogueBaseUrl =
            @"http://172.19.136.129:58940/";

        private static readonly string getProductPathTemplate = "/products?productIds=[{0}]";
        private ICache _cache;

        public ProductCatalogueClient(ICache cache)
        {
            _cache = cache;
        }

        public Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogueIds)
        {
            return ExponentialRetryPolicy.ExecuteAsync(
                async () => await GetItemsFromCatalogueService(productCatalogueIds).ConfigureAwait(false));
        }

        private async Task<IEnumerable<ShoppingCartItem>> GetItemsFromCatalogueService(int[] productCatalogueIds)
        {
            var response = await RequestProductFromProductCatalogue(productCatalogueIds).ConfigureAwait(false);

            return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productCatalogueIds)
        {
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", productCatalogueIds));

            if (_cache.Get(productsResource) is HttpResponseMessage response)
            {
                return response;
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(productCatalogueBaseUrl);
                httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                response = await httpClient.GetAsync(productsResource).ConfigureAwait(false);

                AddToCache(productsResource, response);
                return response;
            }
        }

        private void AddToCache(string resource, HttpResponseMessage response)
        {
            var cacheHeader = response.Headers.FirstOrDefault(h => h.Key == "cache-control");

            if (string.IsNullOrEmpty(cacheHeader.Key))
            {
                return;
            }

            var maxAge =
                CacheControlHeaderValue.Parse(cacheHeader.Value.ToString())
                    .MaxAge;
            if (maxAge.HasValue)
            {
                _cache.Add(key: resource, value: response, ttl: maxAge.Value);
            }
        }

        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(
            HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var products =
                JsonConvert.DeserializeObject<List<ProductCatalogueProduct>>(
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            return products.Select(
                p => new ShoppingCartItem(
                    int.Parse(p.ProductId),
                    p.ProductName,
                    p.ProductDescription,
                    p.Amount,
                    p.Currency
                ));
        }

        private class ProductCatalogueProduct
        {
            public string Currency { get; set; }
            public decimal Amount { get; set; }
            public string ProductDescription { get; set; }
            public string ProductId { get; set; }
            public string ProductName { get; set; }
        }
    }
}