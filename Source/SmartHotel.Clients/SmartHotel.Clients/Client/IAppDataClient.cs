using System.Collections.Generic;
using System.Threading.Tasks;
using Shopanizer.DataObjects;
using System.Reactive;
using System;

namespace Shopanizer.Client
{
    public interface IAppDataClient
    {

        IObservable<User> GetUser();

        IObservable<List<ItemList>> GetItemLists();
        IObservable<List<Item>> GetItems();
        IObservable<Item> SaveItem(Item item);

        IObservable<bool> DeleteItem(Item item);

        IObservable<ShoppingList> SaveShoppingList(ShoppingList shoppingList);

        IObservable<List<ShoppingList>> GetShoppingLists();

        IObservable<bool> UsernameAndPasswordAuthenticate(string username, string password);
        IObservable<User> UsernameAndPasswordRegister(string username, string password);

        IObservable<User> TryToAuthenticate();

        void RefreshUser();
    }
}