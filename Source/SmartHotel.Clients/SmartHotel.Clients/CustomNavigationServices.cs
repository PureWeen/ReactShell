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
        List<Uri> navigationStack = new List<Uri>();
        Page _lastPage;

        public override Page Create(ShellContentCreateArgs args)
        {
            if (args.Content.Route == nameof(TopSecretPage))
                return new TopSecretPage();

            // could even replace this with your own create method
            Page createPage = null;

            #region replace create
            // if (args.Content.Route == nameof(To))
            //    createPage = new LoginPage();
            #endregion

            createPage = createPage ?? base.Create(args);

            string typeName = String.Concat("Shopanizer.", args.Content.Route);
            var viewModel = (BaseViewModel)Activator.CreateInstance(this.GetType().Assembly.GetType(typeName));

            #region back button
            //Shell.SetBackButtonBehavior(createPage, new BackButtonBehavior()
            //{
            //    Command = new Command(async () =>
            //    {
            //        if (navigationStack.Count > 1)
            //        {
            //            await Shell.Current.GoToAsync("..", false);
            //        }
            //        else
            //        {
            //            Shell.Current.FlyoutIsPresented = true;
            //        }
            //    })
            //});
            #endregion

            createPage.BindingContext = viewModel;
            return createPage;
        }



        public override async Task<ShellRouteState> NavigatingToAsync(ShellNavigationArgs args)
        {
            #region tombstone
            //if (navigationStack.Count == 0)
            //{
            //    navigationStack = LoadState();

            //    if(navigationStack.Count > 0)
            //        return await ParseAsync(new ShellUriParserArgs(args.Shell, navigationStack.Last()));
            //}


            //if (args.FutureState.CurrentRoute.FullUri != navigationStack.LastOrDefault())
            //{
            //    navigationStack.Add(args.FutureState.CurrentRoute.FullUri);
            //    SaveState();
            //}
            #endregion

            #region animation
            //var page = args.Shell.RouteState.GetCurrentPage();

            //if (page != null)
            //{
            //    await page.ScaleTo(0, 500);
            //}
            #endregion

            return await base.NavigatingToAsync(args);
        }

        public override async Task AppearingAsync(ShellLifecycleArgs args)
        {
            var incomingPage = args.RoutePath.GetCurrentPage();

            #region backbutton
            //var backButtonBehavior = Shell.GetBackButtonBehavior(incomingPage);

            //if (navigationStack.Count > 1)
            //{
            //    backButtonBehavior.TextOverride = "Back";
            //}
            //else
            //{
            //    backButtonBehavior.TextOverride = null;
            //}
            #endregion

            #region animation
            //if (_lastPage != null)
            //{
            //    incomingPage.Scale = 0;
            //    incomingPage.Rotation = 3600;

            //    Device.BeginInvokeOnMainThread(async () =>
            //    {
            //        await Task.WhenAll(incomingPage.ScaleTo(1, 500), incomingPage.RotateTo(0, 600));
            //    });
            //}
            #endregion

            _lastPage = incomingPage;

            await base.AppearingAsync(args);
        }

        public override async Task<ShellRouteState> ParseAsync(ShellUriParserArgs args)
        {
            #region back button ..
            //if (args.Uri.ToString() == "..")
            //{
            //    var lastPlace = navigationStack.Last();
            //    navigationStack.Remove(lastPlace);

            //    var parsePrevious = await base.ParseAsync(new ShellUriParserArgs(args.Shell, navigationStack.LastOrDefault()));
            //    return parsePrevious;
            //}
            #endregion


            var parseArgs = await base.ParseAsync(args);
            return parseArgs;
        }

        #region saving stack

        void SaveState()
        {
            var uri = String.Join("STAAAAAACK", navigationStack.Select(x => x.ToString()));
            Xamarin.Essentials.Preferences.Set("WhereAmI", uri);
        }

        List<Uri> LoadState()
        {
            try
            {
                var saved = Xamarin.Essentials.Preferences.Get("WhereAmI", null);

                if (saved == null)
                {
                    return new List<Uri>();
                }

                return saved.Split(new[] { "STAAAAAACK" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new Uri(x))
                    .ToList();
            }
            catch
            {
            }

            return new List<Uri>();
        }
        #endregion
    }
}
