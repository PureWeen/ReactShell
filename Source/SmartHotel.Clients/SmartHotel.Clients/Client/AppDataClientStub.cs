using Shopanizer.DataObjects;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Shopanizer.Client
{
    public class AppDataClientStub : IAppDataClient, IEnableLogger
    {

        List<ItemList> _itemList = null;
        List<Item> _items = null;
        List<ShoppingList> _shoppingLists = null;
        User _user = null;

        public AppDataClientStub()
        {

            _shoppingLists = new List<ShoppingList>();
            _itemList = new List<ItemList>()
            {
                new ItemList()
                {
                      Name = "King Stupids"
                },
                new ItemList()
                {
                      Name = "Satan's Way"
                },
            };


            _items = new List<Item>
                     {
                         new Item("1", "You Baby")
                         {
                              Aisle =  new Aisle() { Name = "Animal" } , ItemLists = _itemList.Skip(1).ToList()
                         },
                         new Item("1", "Vampire")
                         {
                              Aisle =  new Aisle() { Name = "Animal" } , ItemLists = _itemList
                         },
                         new Item("1", "Jitter-Bug")
                         {
                              Aisle =  new Aisle() { Name = "Dance" } , ItemLists = _itemList.Skip(1).ToList()
                         },
                         new Item("1", "Rascist")
                         {
                              Aisle =  new Aisle() { Name = "MOnkey" } , ItemLists = _itemList
                         },
                         new Item("1", "Peter")
                         {
                             Aisle =  new Aisle() { Name = "MOnkey" } , ItemLists = _itemList
                         },
                         new Item("1", "blue")
                         {
                              Aisle =  new Aisle() { Name = "Veins" } , ItemLists = _itemList.Skip(1).ToList()
                         },
                         new Item("1", "Alone and tired")
                         {
                              Aisle =  null, ItemLists = _itemList.Skip(1).ToList()
                         }
                     };


            _user = new User()
            {
                Items = _items,
                CurrentShoppingTrip = null
            };
        }



        public IObservable<List<Item>> GetItems()
        {
            var returnValue = Observable.Create<List<Item>>(subj =>
            {
                subj.OnNext(_items);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "GetItems");
        }

        public IObservable<List<ItemList>> GetItemLists()
        {
            var returnValue = Observable.Create<List<ItemList>>(subj =>
            {
                subj.OnNext(_itemList);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "GetItemLists");
        }


        public IObservable<bool> DeleteItem(Item item)
        {
            var returnValue = Observable.Create<bool>(subj =>
            {
                _items.Remove(item);

                subj.OnNext(true);
                subj.OnCompleted();
                return Disposable.Empty;
            });


            return returnValue.Log(this, "DeleteItem");
        }

        public IObservable<Item> SaveItem(Item item)
        {
            var returnValue = Observable.Create<Item>(subj =>
            {

                var existing = _items.FirstOrDefault(x => item.Equals(x));
                if (existing != null)
                {
                    _items.Remove(existing);
                }

                _items.Add(item);

                subj.OnNext(item);
                subj.OnCompleted();
                return Disposable.Empty;
            });


            return returnValue.Log(this, "SaveItem");
        }

        public IObservable<ShoppingList> SaveShoppingList(ShoppingList shoppingList)
        {
            var returnValue = Observable.Create<ShoppingList>(subj =>
            {
                _shoppingLists.Clear();
                _shoppingLists.Add(shoppingList);
                _user.CurrentShoppingTrip = shoppingList;

                subj.OnNext(shoppingList);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            
            return returnValue.Log(this, "SaveShoppingList");
        }


        public IObservable<List<ShoppingList>> GetShoppingLists()
        {
            var returnValue = Observable.Create<List<ShoppingList>>(subj =>
            {
                subj.OnNext(_shoppingLists);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "GetShoppingLists");
        }

        public IObservable<User> GetUser()
        {
            var returnValue = Observable.Create<User>(subj =>
            {
                subj.OnNext(_user);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "GetUser");
        }

        public IObservable<bool> UsernameAndPasswordAuthenticate(string username, string password)
        {
            var returnValue = Observable.Create<bool>(subj =>
            {
                if(_user == null)
                {
                    _user = new User() { UserName = "test", Password = "test" };
                }

                if (username == _user.UserName)
                    subj.OnNext(true);
                else if (password == _user.Password)
                    subj.OnNext(false);

                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "UsernameAndPasswordAuthenticate");
        }

        public IObservable<User> UsernameAndPasswordRegister(string username, string password)
        {
            var returnValue = Observable.Create<User>(subj =>
            {
                _user = new User() { UserName = username, Password = password, Id = Guid.NewGuid().ToString() };

                subj.OnNext(_user);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "UsernameAndPasswordRegister");
        }

        public IObservable<User> TryToAuthenticate()
        {
            var returnValue = Observable.Create<User>(subj =>
            {
                subj.OnNext(null);
                subj.OnCompleted();
                return Disposable.Empty;
            });

            return returnValue.Log(this, "TryToAuthenticate");
        }
        public void RefreshUser()
        {
            _user = null;
        }

    }
}
