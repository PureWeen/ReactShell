using Shopanizer.Client;
using Shopanizer.DataObjects;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Microsoft.WindowsAzure.MobileServices;
using Refit;
using ReactiveUI.Legacy;

namespace Shopanizer.Services
{
    public class AppService : IEnableLogger
    {


        IAppDataClient _appDataclient;
        ICredentialManager _credManager;

        protected AppService(IAppDataClient client, ICredentialManager credManager)
        {
            _appDataclient = client;
            _credManager = credManager;
        }

        public AppService() :
            this(
                Locator.Current.GetService<IAppDataClient>(), 
                Locator.Current.GetService<ICredentialManager>())
        {
            
        }


        public IObservable<User> GetUser()
        {
            return _appDataclient.GetUser()
                .Select(user =>
                {

                    //TODO Why you null? :-(
                    var appState = RxApp.SuspensionHost.GetAppState<AppBootStrapper>();
                    if (appState != null)
                    {
                        appState.UserId = user.Id;
                        appState.UserName = user.UserName;
                    }
                    return user;
                })
                .LoggedCatchExt(this, (Exception exc) => Observable.Return<User>(new User() { ServerErrorMessage = exc.ToString() }));
        }

        public IObservable<User> UsernameAndPasswordRegister(string userName, string password)
        {
            return _appDataclient.UsernameAndPasswordRegister(userName, password);
        }


        public IObservable<User> TryToAuthenticate()
        {
            return _appDataclient.TryToAuthenticate();  
        }



        public IObservable<bool> UsernameAndPasswordAuthenticate(string username, string password)
        {
            return _appDataclient.UsernameAndPasswordAuthenticate(username, password);
        }

        public IObservable<List<ItemList>> GetItemLists()
        {
            var returnValue = _appDataclient.GetItemLists()
                .Select(il =>
                {
                    il = il.OrderBy(x => x.Name).ToList();
                    return il;
                })
                .LoggedCatchExt(this, (Exception exc) => Observable.Return<List<ItemList>>(new List<ItemList>()));

            return returnValue;
        }

        internal void RefreshUser()
        {
            _appDataclient.RefreshUser();
        }

        public IObservable<IReactiveList<KeyValuePair<string, ItemList>>> GetItemListsSelectList()
        {
            

            return GetItemLists()
                    .Select(itemLists =>
                    {

                        var returnValue  = itemLists.ToSelectItemList<ItemList>(item => item.Name, "No List");
                        return returnValue;
                    })
                .LoggedCatchExt(this, (Exception exc) => Observable.Return(new ReactiveList<KeyValuePair<string, ItemList>>()));
        }


        public IObservable<List<Aisle>> GetAisles()
        {
            return _appDataclient.GetItems()
                .Select(items =>
                {
                    var returnValue =  items.Select(x => x.Aisle).Where(x=> x != null).Distinct().OrderBy(x=> x.Name).ToList();

                    return returnValue;
               
                })
                .LoggedCatchExt(this, (Exception exc) => Observable.Return<List<Aisle>>(new List<Aisle>()));
        }

        public IObservable<bool> DeleteItem(Item item)
        {
            return _appDataclient.DeleteItem(item);
        }

        public IObservable<Item> SaveItem(Item item)
        {
            return _appDataclient.SaveItem(item);
        }

        public IObservable<List<Item>> GetItems()
        {
            return _appDataclient.GetItems();
        }




        public IObservable<ShoppingList> SaveShoppingList(ShoppingList list)
        {
            return _appDataclient.SaveShoppingList(list);
        }



        public IObservable<ShoppingList> GetShoppingTrip()
        {
            return _appDataclient.GetShoppingLists().Select(x => x.FirstOrDefault())
                .LoggedCatchExt(this, (Exception exc) => Observable.Return<ShoppingList>(new ShoppingList { ServerErrorMessage = exc.ToString() }));
        }

        public IObservable<ShoppingList> StartShoppingTrip(ItemList list)
        {
            var returnValue = GetItems()
                .Select(items => items.Where(item => item.Need && item.ItemLists.Contains(list)))
                .Select(items =>
                {
                    ShoppingList shoppingList = new ShoppingList();
                    shoppingList.Name = list.Name;
                    shoppingList.Description = list.Description;
                    shoppingList.ShoppingItems = items.Where(item => item.Need).Select(item => new ShoppingItem(item)).ToList();
                    shoppingList.IsCompleted = false;
                    shoppingList.ItemList = list;
                    return shoppingList;
                })
                .SelectMany(sl=> _appDataclient.SaveShoppingList(sl))
                .LoggedCatchExt(this, (Exception exc) => Observable.Return<ShoppingList>(new ShoppingList { ServerErrorMessage = exc.ToString() }));

            return returnValue;
        }
    }
}
