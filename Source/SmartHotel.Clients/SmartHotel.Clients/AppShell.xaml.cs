using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.ComponentModel;
using Xamarin.Forms.Xaml;
using SmartHotel.Clients.Core.Services;

namespace SmartHotel.Clients.Core
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            this.SetNavigationService(new CustomNavigationServices());
            InitializeComponent();
        }
    }
}