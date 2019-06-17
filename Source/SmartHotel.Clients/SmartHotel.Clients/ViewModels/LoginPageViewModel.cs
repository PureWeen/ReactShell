using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ReactiveUI;

namespace Shopanizer.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {

        public ICommand LoginCommand { get; }

        public LoginPageViewModel()
        {
            LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    await CustomNavigationServices.GotoAsync(new HomePageViewModel() { Text = "View Model Navigation yay" });
                }
                catch(Exception exc)
                {
                }
            });
        }
    }
}
