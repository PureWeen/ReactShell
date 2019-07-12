using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Shopanizer
{
    public class LoginPageViewModel : BaseViewModel
    {
        public LoginPageViewModel()
        {
            LoginCommand  =
               new Command(async () =>
               {
                   if(!IsAdmin)
                        await Shell.Current.GoToAsync($"//{nameof(ItemsPageViewModel)}");
                   else
                   {
                       (Shell.Current.BindingContext as ShellViewModel).IsAdmin = IsAdmin;
                       await Shell.Current.GoToAsync($"//{nameof(TopSecretPage)}");
                   }
               }
           );
        }

        public ICommand LoginCommand { get; }

        public bool IsAdmin
        {
            get;
            set;
        }
    }
}
