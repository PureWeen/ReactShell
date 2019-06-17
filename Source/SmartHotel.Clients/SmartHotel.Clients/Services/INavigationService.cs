using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Shopanizer.Services
{
    public enum NavigationDestinations
    {
        Unknown, UserItemListView, AddUserItemView, LoginView,
        ShoppingListView
    }

    public interface INavigationService
    {
        IObservable<Unit> GoBack();
        void Navigate(NavigationDestinations destination);
        void Navigate(NavigationDestinations destination, bool keepMeInNavigationStack);
        void NavigateAndReset(NavigationDestinations destination);
        IObservable<bool> DisplayAlert(string title, string message, string accept, string cancel);

    }

    

}