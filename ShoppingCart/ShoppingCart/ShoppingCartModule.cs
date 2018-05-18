using Nancy;
using Nancy.ModelBinding;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartModule : NancyModule
    {
        public ShoppingCartModule(
            IShoppingCartStore shoppingCartStore,
            IProductcatalogClient productcatalog,
            IEventStore eventStore) : base("/shoppingcart")
        {
            Get(
                "/{userid:int}",
                parameters =>
                {
                    var userId = (int)parameters.userid;
                    return shoppingCartStore.Get(userId);
                });
            Post("/{userid:int}/items",
                async (parameters, _) =>
                {
                    var productcatalogIds = this.Bind<int[]>();
                    var userId = (int)parameters.userid;

                    var shoppingCart = shoppingCartStore.Get(userId);
                    var shoppingCartItems = await productcatalog.GetShoppingCartItems(productcatalogIds)
                        .ConfigureAwait(false);
                    shoppingCart.AddItems(shoppingCartItems, eventStore);
                    shoppingCartStore.Save(shoppingCart);
                });
        }
    }
}