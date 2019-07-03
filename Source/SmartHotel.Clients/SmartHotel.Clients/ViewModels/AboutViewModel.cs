using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace Shopanizer
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://xamarin.com/platform")));

            GoBack = new Command(async () => await Shell.Current.GoToAsync("//ItemsPageViewModel"));
        }

        public ICommand OpenWebCommand { get; }
        public ICommand GoBack { get; }
    }
}