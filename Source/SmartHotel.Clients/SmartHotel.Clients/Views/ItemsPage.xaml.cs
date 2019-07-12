using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Shopanizer.Models;
using Shopanizer;

namespace Shopanizer
{
    public partial class ItemsPage : ContentPage
    {
        ItemsPageViewModel viewModel => (ItemsPageViewModel)BindingContext;

        public ItemsPage()
        {
            InitializeComponent();
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()), false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BindingContext = BindingContext;

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}