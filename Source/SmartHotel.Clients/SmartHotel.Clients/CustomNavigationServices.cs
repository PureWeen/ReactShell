using Shopanizer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;

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
        }

        public override Page Create(ShellContentCreateArgs args)
        {
            // could even replace this with your own create method
            Page createPage = base.Create(args);
            string typeName = String.Concat("Shopanizer.", args.Content.Route);
            var viewModel = Activator.CreateInstance(this.GetType().Assembly.GetType(typeName));
            createPage.BindingContext = viewModel;
            return createPage;
        }

        public override async Task<ShellRouteState> ParseAsync(ShellUriParserArgs args)
        {
            var uri = args.Uri;
            if(initialLoad)
            {
                initialLoad = false;
                var saved = Xamarin.Essentials.Preferences.Get("WhereAmI", null);

                if(!String.IsNullOrWhiteSpace(saved))
                {
                    try
                    {
                        var savedArgs = await base.ParseAsync(new ShellUriParserArgs(args.Shell, new Uri(saved, UriKind.RelativeOrAbsolute)));
                        return savedArgs;
                    }
                    catch
                    {

                    }
                }
            }

            if(args.Uri.ToString() == "..")
            {
                var currentState = args.Shell.RouteState;
                currentState.CurrentRoute.PathParts = new ReadOnlyCollection<PathPart>(currentState.CurrentRoute.PathParts.Reverse().Skip(1).Reverse().ToList());
                return currentState;
            }

            var parseArgs = await base.ParseAsync(args);
            SaveState(parseArgs.CurrentRoute.FullUri.ToString());
            return parseArgs;
        }

        bool initialLoad = true;
        public override Task<ShellRouteState> NavigatingToAsync(ShellNavigationArgs args)
        {   
            var currentPath = args.FutureState.CurrentRoute.PathParts.Last();
            return base.NavigatingToAsync(args);
        }

        void SaveState(string uri)
        {
            Xamarin.Essentials.Preferences.Set("WhereAmI", uri);
        }
    }
}
