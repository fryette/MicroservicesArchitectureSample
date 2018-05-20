using System.Threading.Tasks;

namespace ShoppingCart.ShoppingCart
{
    public interface IShoppingCartStore
    {
        Task<ShoppingCart> GetAsync(int userId);
        Task SaveAsync(ShoppingCart shoppingCart);
    }
}