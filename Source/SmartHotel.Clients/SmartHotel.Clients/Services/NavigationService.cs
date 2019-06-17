using ReactiveUI;
using ReactiveUI.XamForms;
using Shopanizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

namespace Shopanizer.Services
{
    public class NavigationService : INavigationService
    {
        IScreen _hostScreen;
        RoutedViewHost _RoutedViewHost;

        public NavigationService(IScreen hostScreen, RoutedViewHost routedViewHost)
        {
            _hostScreen = hostScreen;
            _RoutedViewHost = routedViewHost;
        }

        public void NavigateAndReset(NavigationDestinations destination)
        {
            _RoutedViewHost.Router.NavigateAndReset
                .Execute(new UserItemListViewModel())
                .Subscribe();
        }

        public IObservable<bool> DisplayAlert(string title, string message, string accept, string cancel)
        {
            return Observable.FromAsync(() =>  _RoutedViewHost.DisplayAlert(title, message, accept, cancel));
        }
        public IObservable<Unit> GoBack()
        {
            //HACK:SHANE Doesn't look like this works with WINRT right now
            // return _hostScreen.Router.NavigateBack.ExecuteAsync(Unit.Default);

            return  Observable.FromAsync(() => _RoutedViewHost.Navigation.PopAsync())
                .Select(_=> Unit.Default);

            //return true;
        }

        public void Navigate(NavigationDestinations destination)
        {
            Navigate(destination, true);
        }

        public void Navigate(NavigationDestinations destination, bool keepMeInNavigationStack)
        {
			IObservable<IRoutableViewModel> command = null;

            var navCommand = _hostScreen.Router.NavigateAndReset;

            if(keepMeInNavigationStack)
            {
                navCommand = _hostScreen.Router.Navigate;
            }
            switch (destination)
            {
				case NavigationDestinations.LoginView:
					command = navCommand.Execute(new LoginViewModel());
                    break;
                case NavigationDestinations.UserItemListView:
					command = navCommand.Execute(new UserItemListViewModel());
                    break;
                case NavigationDestinations.AddUserItemView:
					command = navCommand.Execute(new UserItemDetailsViewModel());
                    break;
                case NavigationDestinations.ShoppingListView:
					command = navCommand.Execute(new ShoppingListViewModel());
                    break;
                default:
                    throw new InvalidOperationException(destination.ToString());
            }


			command.Subscribe ();
            
        }
    }
}
