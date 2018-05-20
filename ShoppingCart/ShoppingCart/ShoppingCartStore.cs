using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ShoppingCart.ShoppingCart.Models;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartStore : IShoppingCartStore
    {
        private string connectionString =
            @"Data Source=EPBYBREW0024\SQLEXPRESS;Initial Catalog=ShoppingCart;Integrated Security=True";

        private const string READ_ITEMS_SQL =
            @"select * from ShoppingCart, ShoppingCartItems
where ShoppingCartItems.ShoppingCartId = ShoppingCart.ID
and ShoppingCart.UserId=@UserId";

        private const string DELETE_ALL_FOR_SHOPPING_CART_SQL =
            @"delete item from ShoppingCartItems item
inner join ShoppingCart cart on item.ShoppingCartId = cart.ID
and cart.UserId=@UserId";
        //(SELECT id from ShoppingCart WHERE UserId = 123)
        private const string ADD_ALL_FOR_SHOPPING_CART_SQL =
            @"insert ShoppingCartItems 
(ShoppingCartId, ProductCatalogId, ProductName, ProductDescription, Amount, Currency)
values 
((SELECT id from ShoppingCart WHERE UserId = @UserId), @ProductCatalogId, @ProductName,@ProductDescription, @Amount, @Currency)";

        public async Task<ShoppingCart> GetAsync(int userId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var items = await
                    conn.QueryAsync<ShoppingCartItem>(
                        READ_ITEMS_SQL,
                        new { UserId = userId });

                conn.Close();

                return new ShoppingCart(userId, items);
            }
        }

        public async Task SaveAsync(ShoppingCart shoppingCart)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    await conn.ExecuteAsync(
                        DELETE_ALL_FOR_SHOPPING_CART_SQL,
                        new { UserId = shoppingCart.UserId },
                        tx).ConfigureAwait(false);

                    var objects = shoppingCart.Items.Select(x => new
                    {
                        UserId = shoppingCart.UserId,
                        x.ProductCatalogId,
                        x.ProductName,
                        x.ProductDescription,
                        x.Amount,
                        x.Currency
                    });

                    conn.Execute(ADD_ALL_FOR_SHOPPING_CART_SQL, objects, tx);

                    tx.Commit();
                }

                conn.Close();
            }
        }
    }
}