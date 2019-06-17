using AutoMapper;
using Microsoft.WindowsAzure.MobileServices; 
using ReactiveUI;
using Refit;
using Shopanizer.DataObjects;
using Shopanizer.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Shopanizer.Client
{
    public class AppDataClient : IAppDataClient, IEnableLogger
    {

#if DEBUG
   /*     public static MobileServiceClient MobileService = new MobileServiceClient(
                  "https://localhost:44307/", "pants"
        );*/

        public static MobileServiceClient MobileService = new MobileServiceClient(
            "https://shopanizer.azure-mobile.net/",
            "ZMaWZadKTeeeMKlEtPmxjMiFaYoOCG75"
      );

        //    awiulhialwuehfhilawuef;
#else
        // Use this constructor instead after publishing to the cloud
        public static MobileServiceClient MobileService = new MobileServiceClient(
             "https://shopanizer.azure-mobile.net/",
             "ZMaWZadKTeeeMKlEtPmxjMiFaYoOCG75"
       );
#endif

        IAuthenticationClient _authClient;
        ICredentialManager _credentialManager;
        Subject<ShoppingList> _saveShoppingListThrottle;
        Subject<ShoppingList> _saveShoppingListSaveNow;
        IObservable<ShoppingList> _shoppingListSaved;

        public AppDataClient()
        {            
        }
    
		public void Initialize()
		{
			var client = new HttpClient()
			{
				BaseAddress = MobileService.ApplicationUri
			};

			_authClient =  RestService.For<IAuthenticationClient>(client);
			_credentialManager = Locator.Current.GetService<ICredentialManager>();
			_saveShoppingListThrottle = new Subject<ShoppingList>();
			_saveShoppingListSaveNow = new Subject<ShoppingList>();

			_shoppingListSaved = 
				Observable.Amb(_saveShoppingListThrottle.Throttle(TimeSpan.FromSeconds(5)), _saveShoppingListSaveNow)
					.SelectMany(shoppingList =>
						Observable.FromAsync(() => MobileService.InvokeApiAsync<ShoppingList, ShoppingList>("ShoppingList", shoppingList))
						.Select(sl =>
							{
								//TODO check logic of setting this is it ok?
								if (_user != null)
								{
									if (sl.IsCompleted)
										_user.CurrentShoppingTrip = null;
									else
										_user.CurrentShoppingTrip = sl;
								}

								Mapper.Map<Resource, Resource>(sl, shoppingList);
								return sl;
							}))
					.Repeat()
					.LoggedCatchExt(this, (Exception exc) => Observable.Return<ShoppingList>(new ShoppingList() { ServerErrorMessage = exc.ToString() }))
					.Publish()
					.RefCount();

			_shoppingListSaved.Subscribe();
		}

        public IObservable<User> UsernameAndPasswordRegister(string username, string password)
        {
            ClearUser();
            var returnValue = _authClient.UserPost(new User { UserName = username, Password = password  })
                .Select(result =>
                {
                    return result;
                }
            ).LoggedCatchExt(this, (ApiException e) => Observable.Return(new User() { ServerErrorMessage = e.Content }));

            return returnValue;
        }


        public IObservable<bool> UsernameAndPasswordAuthenticate(string username, string password)
        {
            ClearUser();
            var returnValue = _authClient.Login(new Requests.LoginRequest { username = username, password = password })
                .Select(result =>
                {
                    if (!String.IsNullOrWhiteSpace(result.MobileServiceAuthenticationToken))
                    {
                        MobileServiceUser user = new MobileServiceUser(result.UserId);
                        user.MobileServiceAuthenticationToken = result.MobileServiceAuthenticationToken;
                        MobileService.CurrentUser = user;

                        StoredCredentials cred = new StoredCredentials() { UserName = result.UserId, Password = result.MobileServiceAuthenticationToken };

                        _credentialManager.StoreCredentials("UsernameAndPassword", cred);

                        return true;
                    }

                    return false;                    
                }
            ).LoggedCatchExt(this, (Exception e) => Observable.Return(false));

            return returnValue;
        }



        public IObservable<User> TryToAuthenticate()
        {
            if(MobileService.CurrentUser == null)
            {                
                var  credentials = _credentialManager.GetTokenByResource("UsernameAndPassword");

                if (credentials != null)
                {
                    //username is actually the userid
                    MobileServiceUser user = new MobileServiceUser(credentials.UserName);
                    user.MobileServiceAuthenticationToken = credentials.Password;
                    MobileService.CurrentUser = user;

                    return GetUser().LoggedCatchExt(this, (Microsoft.WindowsAzure.MobileServices.MobileServiceInvalidOperationException e) =>
                    {
                        MobileService.CurrentUser = null;
                        _credentialManager.StoreCredentials("UsernameAndPassword", null);
                        return Observable.Return<User>(null);
                    });
                }

            }

            return GetUser();
        }


        User _user = null;
        public IObservable<User> GetUser()
        {
            if (MobileService.CurrentUser == null)
                return Observable.Return<User>(null);

            if (_user != null)
                return Observable.Return(_user);

            return Observable.FromAsync(() => MobileService.InvokeApiAsync<User>("User", HttpMethod.Get, null))
                .Select(user => { _user = user; return user; });

        }



        public IObservable<List<ItemList>> GetItemLists()
        {
            return GetItems()
                .Select(x =>
                {
                    return x.SelectMany(il => il.ItemLists).Distinct().ToList();
                });
        }

        public IObservable<List<Item>> GetItems()
        {
            return GetUser()
               .Select(x =>
               {
                   return x.Items;
               });
        }

        public IObservable<List<ShoppingList>> GetShoppingLists()
        {
            return GetUser()
               .Select(x =>
               {
                   return new List<ShoppingList> { x.CurrentShoppingTrip };
               });
        }

        public IObservable<bool> DeleteItem(Item item)
        {
            var parameters = new Dictionary<string, string> { {"id", item.Id } };

            return Observable.FromAsync(() => MobileService.InvokeApiAsync("Item", HttpMethod.Delete, parameters))
                .Select(_ =>
                {
                    if (_user != null && _user.Items.Contains(item))
                    {
                        _user.Items.Remove(item);
                    }

                    return true;
                })
                .LoggedCatchExt(this, (Exception exc) => Observable.Return(false));
        }


        public IObservable<Item> SaveItem(Item item)
        {
            return Observable.FromAsync(() => MobileService.InvokeApiAsync<Item, Item>("Item", item))
                .Select(newItem =>
                {
                    if(_user != null && !_user.Items.Contains(item))
                    {
                        _user.Items.Add(item);
                    }

                    Mapper.Map(newItem, item);
                    return item;
                })
                .LoggedCatchExt(this, (Exception exc) => Observable.Return<Item>(null));
        }



        public IObservable<ShoppingList> SaveShoppingList(ShoppingList shoppingList)
        {
            return Observable.Create<ShoppingList>(sub =>
            {
                if (_user.CurrentShoppingTrip == null || shoppingList.IsCompleted)
                {
                    _saveShoppingListSaveNow.OnNext(shoppingList);
                    _user.CurrentShoppingTrip = shoppingList;
                }
                else
                {                 
                    _saveShoppingListThrottle.OnNext(shoppingList);
                }

                return _shoppingListSaved.Take(1).Log(this, "_shoppingListSaved").Subscribe(sub);
            }).LoggedCatchExt(this, (Exception exc) => Observable.Empty<ShoppingList>(), "SaveShoppingList");
        }

        public void RefreshUser()
        {
            _user = null;
        }

        private void ClearUser()
        {

            _user = null;
            MobileService.CurrentUser = null;
        }
    }
}
