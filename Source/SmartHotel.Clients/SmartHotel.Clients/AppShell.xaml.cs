using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.ComponentModel;
using Xamarin.Forms.Xaml;

namespace Shopanizer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            BindingContext = new ShellViewModel();
            Routing.RegisterRoute("ItemDetailPageViewModel", typeof(ItemDetailPage));
            this.SetNavigationService(new CustomNavigationServices());
            InitializeComponent();
        }
    }
}