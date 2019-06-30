using Shopanizer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;

namespace Shopanizer
{
    public class CustomNavigationServices : ShellNavigationService
    {
        public CustomNavigationServices()
        {

        }

        public override void ApplyParameters(ShellLifecycleArgs args)
        {
            base.ApplyParameters(args);

            if(args.Element is LoginPage page)
            {
            }
        }

        public override Page Create(ShellContentCreateArgs args)
        {
            Page createPage = null;
            if (args.Content.Route == "LoginPageViewModel")
            {
                createPage = new LoginPage();
            }
            else
                createPage = base.Create(args);

            if(createPage is LoginPage)
            {

            }

            return createPage;
        }

        public override async Task<ShellRouteState> ParseAsync(ShellUriParserArgs args)
        {
            var parseArgs = await base.ParseAsync(args);
            return parseArgs;
        }

        public override Task<ShellRouteState> NavigatingToAsync(ShellNavigationArgs args)
        {
            var currentPath = args.FutureState.CurrentRoute.PathParts.Last();

            if(currentPath.Path == "LoginPageViewModel")
            {
                currentPath.NavigationParameters.Add("BindingContext", new LoginViewModel());
            }

            return base.NavigatingToAsync(args);
        }
    }
}
