using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Shopanizer
{
    public class LoginViewModel : ViewModelBase
    {
        public ICommand LoginCommand { get; } = 
            new Command(() => 
            {
            }
        );
    }
}
