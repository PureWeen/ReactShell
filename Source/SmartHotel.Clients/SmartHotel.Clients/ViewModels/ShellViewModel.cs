using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Shopanizer
{
    public class ShellViewModel : BaseViewModel
    {
        bool _isAdmin;
        public ShellViewModel()
        {
            LogoutCommand =
                new Command(async () =>
                {
                    IsAdmin = false;
                    await Shell.Current.GoToAsync($"//{nameof(LoginPageViewModel)}");
                }
            );
        }

        public bool IsAdmin
        {
            get
            {
                return _isAdmin;
            }
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LogoutCommand { get; } 
    }
}
