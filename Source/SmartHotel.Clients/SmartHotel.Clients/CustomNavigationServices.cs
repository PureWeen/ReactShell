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

        public override async Task<ShellRouteState> NavigatingToAsync(ShellNavigationArgs args)
        {
            var currentPath = args.FutureState.CurrentRoute.PathParts.Last();
            var activePart = args.Shell.RouteState.CurrentRoute.PathParts.LastOrDefault();
            var page = (activePart?.ShellPart as IShellContentController)?.Page;

            if (page != null)
            {
                await page.ScaleTo(0, 500);
            }

            return await base.NavigatingToAsync(args);
        }

        Page _lastPage;
        public override async Task AppearingAsync(ShellLifecycleArgs args)
        {
            var incomingPage = (args.RoutePath.PathParts.LastOrDefault()?.ShellPart as IShellContentController)?.Page;

            if (_lastPage != null)
            {
                incomingPage.Scale = 0;
                incomingPage.Rotation = 3600;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.WhenAll(incomingPage.ScaleTo(1, 500), incomingPage.RotateTo(0, 600));
                });
            }

            _lastPage = incomingPage;           

            await base.AppearingAsync(args);
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
                SaveState(currentState.CurrentRoute.FullUri.ToString());
                return currentState;
            }

            var parseArgs = await base.ParseAsync(args);
            SaveState(parseArgs.CurrentRoute.FullUri.ToString());
            return parseArgs;
        }

        bool initialLoad = true;

        void SaveState(string uri)
        {
            Xamarin.Essentials.Preferences.Set("WhereAmI", uri);
        }
    }
}
