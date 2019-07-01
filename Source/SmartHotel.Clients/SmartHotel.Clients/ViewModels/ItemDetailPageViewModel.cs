using System;
using Xamarin.Forms;

using Shopanizer.Models;
using Shopanizer.Services;

namespace Shopanizer
{
    [QueryProperty("Id", "Id")]
    public class ItemDetailPageViewModel : BaseViewModel
    {
        private string id;
        private Item item;

        public string Id
        {
            get => id;
            set
            {
                id = value;
                LoadData(value);
            }
        }

        async void LoadData(string value)
        {
            SetItem(await DataStore.GetItemAsync(value));
        }

        public Item Item
        {
            get => item;
            set
            {
                item = value;
                OnPropertyChanged();
            }
        }

        public ItemDetailPageViewModel()
        {
        }

        void SetItem(Item item)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}
