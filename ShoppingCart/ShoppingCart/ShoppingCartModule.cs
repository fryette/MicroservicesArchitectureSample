using Nancy;
using Nancy.ModelBinding;
using ShoppingCart.EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public sealed class ShoppingCartModule : NancyModule
    {
        public ShoppingCartModule(
            IShoppingCartStore shoppingCartStore,
            IProductCatalogueClient productcatalog,
            IEventStore eventStore) : base("/shoppingcart")
        {
            Get(
                "/{userid:int}",
                parameters =>
                {
                    var userId = (int) parameters.userid;
                    return shoppingCartStore.GetAsync(userId);
                });

            Post(
                "/{userid:int}/items",
                async (parameters, _) =>
                {
                    var productcatalogIds = this.Bind<int[]>();
                    var userId = (int) parameters.userid;

                    var shoppingCart = await shoppingCartStore.GetAsync(userId);

                    var shoppingCartItems = await productcatalog.GetShoppingCartItems(productcatalogIds)
                        .ConfigureAwait(false);
                    shoppingCart.AddItems(shoppingCartItems, eventStore);
                    await shoppingCartStore.SaveAsync(shoppingCart);
                    return shoppingCart;
                });

            Delete(
                "/{userid:int}/items",
                async parameters =>
                {
                    var productCatalogIds = this.Bind<int[]>();
                    var userId = (int) parameters.userid;

                    var shoppingCart = await shoppingCartStore.GetAsync(userId);
                    shoppingCart.RemoveItems(productCatalogIds, eventStore);
                    await shoppingCartStore.SaveAsync(shoppingCart);

                    return shoppingCart;
                });
        }
    }
}