﻿using Shopanizer.Services;
using Shopanizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Shopanizer.Services
{
    public class CustomNavigationServices : ShellNavigationService
    {
        ViewModelBase _navigatingViewModel;

        public CustomNavigationServices()
        {

        }

        public async Task GotoAsync(ViewModelBase viewModelBase)
        {
            _navigatingViewModel = viewModelBase;
            await Shell.Current.GoToAsync($"//{viewModelBase.GetType().Name}", true);
        }

        public override Page Create(ShellContentCreateArgs args)
        {
            var content =  base.Create(args);
            if (args.Content.Route == "HomePageViewModel")
                content.BindingContext = _navigatingViewModel as HomePageViewModel ?? new HomePageViewModel();
            else if (args.Content.Route == "LoginPageViewModel")
                content.BindingContext = _navigatingViewModel as LoginPageViewModel ?? new LoginPageViewModel();
            
            _navigatingViewModel = null;
            return content;
        }
    }
}
