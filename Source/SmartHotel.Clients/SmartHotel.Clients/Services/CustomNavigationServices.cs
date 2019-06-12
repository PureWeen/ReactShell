using SmartHotel.Clients.Core.Services;
using SmartHotel.Clients.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

[assembly: Dependency(typeof(CustomNavigationServices))]
namespace SmartHotel.Clients.Core.Services
{
    public class CustomNavigationServices : ShellNavigationService
    {
        public CustomNavigationServices()
        {

        }

        public override Page Create(ShellContentCreateArgs args)
        {
            var content =  base.Create(args);
            if (args.Content.Route == "HomePageViewModel")
                content.BindingContext = new HomePageViewModel();

            return content;
        }
    }
}
