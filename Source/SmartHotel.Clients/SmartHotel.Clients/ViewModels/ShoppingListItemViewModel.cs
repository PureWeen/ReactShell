using ReactiveUI;
using Shopanizer.DataObjects;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Shopanizer.ViewModels
{
    public partial class ShoppingListViewModel
    {
        

        public class ShoppingListItemViewModel : ReactiveObject
        {
            bool _Need;
            public ShoppingItem Item
            {
                get; set;
            }

            public ShoppingListItemViewModel(ShoppingItem item, IObservable<ShoppingList> procuredChanged)
            {
                Name = item.Name;
                Aisle = item.Aisle;
                Procured = item.Procured;
                Item = item;

                this.WhenAnyValue(x => x.Procured)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Where(x => x != Item.Procured)
                    .SelectMany(procured =>
                    {
                        Item.Procured = procured;
                        return procuredChanged;
                    })
                    .Subscribe();
            }

            public String Name { get; set; }
            public Aisle Aisle { get; set; }
            public bool Procured { get { return _Need; } set { this.RaiseAndSetIfChanged(ref _Need, value); } }

        }
    }
}
