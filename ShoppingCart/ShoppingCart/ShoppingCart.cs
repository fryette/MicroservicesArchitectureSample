using System.Collections.Generic;
using System.Linq;
using ShoppingCart.EventFeed;
using ShoppingCart.ShoppingCart.Models;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        private readonly HashSet<ShoppingCartItem> _items = new HashSet<ShoppingCartItem>();

        public int UserId { get; }
        public IEnumerable<ShoppingCartItem> Items => _items;

        public ShoppingCart(int userId)
        {
            UserId = userId;
        }

        public ShoppingCart(int userId, IEnumerable<ShoppingCartItem> items)
        {
            UserId = userId;

            foreach (var item in items)
            {
                _items.Add(item);
            }
        }

        public void AddItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
            {
                if (_items.Add(item))
                {
                    eventStore.Raise("ShoppingCartItemAdded", new {UserId, item});
                }
            }
        }

        public void RemoveItems(int[] productCatalogueIds, IEventStore eventStore)
        {
            _items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogId));
        }
    }
}