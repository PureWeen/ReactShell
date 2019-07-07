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

        #region SaveState
        List<Uri> navigationStack = new List<Uri>();
        const string stackSeparator = "STAAAAAACK";
        const string stackSetting = "STAAAAAACK";
        void SaveState()
        {
            var uri = String.Join(stackSeparator, navigationStack.Select(x => x.ToString()));
            Xamarin.Essentials.Preferences.Set(stackSetting, uri);
        }

        List<Uri> LoadState()
        {
            try
            {
                var saved = Xamarin.Essentials.Preferences.Get(stackSetting, null);

                if (saved == null)
                {
                    return new List<Uri>();
                }

                return saved.Split(new[] { stackSeparator }, StringSplitOptions.RemoveEmptyEntries)
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
