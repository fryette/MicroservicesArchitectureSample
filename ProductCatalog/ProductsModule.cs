using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;

namespace ProductCatalog
{
    public sealed class ProductsModule : NancyModule
    {
        public ProductsModule(ProductStore productStore) : base("/products")
        {
            Get("", _ =>
            {
                string productIdsString = Request.Query.productIds;
                var productIds = ParseProductIdsFromQueryString(productIdsString);
                var products = productStore.GetProductsByIds(productIds);

                return
            Negotiate
             .WithModel(products)
             .WithHeader("cache-control", "max-age:86400")
                .WithStatusCode(HttpStatusCode.OK);
            });
        }

        private static IEnumerable<int> ParseProductIdsFromQueryString(string productIdsString)
        {
            return productIdsString.Split(',').Select(s => s.Replace("[", "").Replace("]", "")).Select(int.Parse);
        }
    }

    public interface ProductStore
    {
        IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds);
    }

    public class StaticProductStore : ProductStore
    {
        public IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds)
        {
            return productIds.Select(id => new ProductCatalogProduct(id, "foo" + id, "bar", 25, "Euro"));
        }
    }

    public class ProductCatalogProduct
    {
        public ProductCatalogProduct(int productId, string productName, string description, decimal amount, string currency)
        {
            ProductId = productId.ToString();
            ProductName = productName;
            ProductDescription = description;
            Amount = amount;
            Currency = currency;
        }
        public string ProductId { get; }
        public string ProductName { get; }
        public string ProductDescription { get; }
        public decimal Amount { get; }
        public string Currency { get; }
    }
}