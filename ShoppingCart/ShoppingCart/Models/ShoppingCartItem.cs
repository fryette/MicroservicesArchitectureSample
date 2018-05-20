namespace ShoppingCart.ShoppingCart.Models
{
    public class ShoppingCartItem
    {
        public string ProductDescription { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public int ProductCatalogId { get; set; }
        public string ProductName { get; set; }

        public ShoppingCartItem()
        {
            
        }

        public ShoppingCartItem(
            int productCatalogId,
            string productName,
            string description,
            decimal amount,
            string currency)
        {
            ProductCatalogId = productCatalogId;
            ProductName = productName;
            ProductDescription = description;
            Amount = amount;
            Currency = currency;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return obj is ShoppingCartItem that && ProductCatalogId.Equals(that.ProductCatalogId);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return ProductCatalogId.GetHashCode();
        }
    }
}