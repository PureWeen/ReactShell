using ReactiveUI;
using ReactiveUI.Legacy;
using Shopanizer.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopanizer.ViewModels
{
    public partial class ShoppingListViewModel : ViewModelBase
    {
        AlphaKeyGroupList<ShoppingListItemViewModel> _ItemGroups;
        ShoppingList _shoppingList;

        public ShoppingListViewModel()
        {
            //setup lists
            Items = new ReactiveList<ShoppingListItemViewModel>();


            _ItemGroups = new AlphaKeyGroupList<ShoppingListItemViewModel>(
                item => (item.Aisle != null) ? item.Aisle.Name : String.Empty, _ => true,
                Observable.Never<Unit>());



            #region commands



            var loadItems = AppService.GetShoppingTrip()
                    .Select(shopping =>
                    {
                        ShoppingList = shopping;

                        SelectedItemList = shopping.ItemList;
                        foreach (var i in shopping.ShoppingItems)
                        {
                            var closureI = i;

                            var itemVm = new ShoppingListItemViewModel(closureI, AppService.SaveShoppingList(shopping));
                            Items.Add(itemVm);
                        }

                        return Unit.Default;
                    });

			RefreshItemList = ReactiveCommand.CreateFromObservable<Unit,Unit>(_ =>
                loadItems
            );



			CompleteShopTheList = ReactiveCommand.CreateFromObservable<Unit,Unit>(
                _ =>
                {

                    ShoppingList.IsCompleted = true;
                    return AppService.SaveShoppingList(ShoppingList)
                        .Select(result =>
                        {
                            if(!result.IsError)
                            {
                                AppService.RefreshUser();
                                NavigateTo(Services.NavigationDestinations.UserItemListView, null, false);
                            }

                            return Unit.Default;
                        });
                }
            );

            #endregion



            #region eventing and things

            this.OnFirstLoaded
                .SelectMany(_ => RefreshItemList.Execute())
                .Subscribe();



            Items
                .ItemsAdded
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(item =>
                {
                    _ItemGroups.Add(item);
                    return Unit.Default;
                })
                .Subscribe();


            Items
                .ItemsRemoved
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(item =>
                {
                    _ItemGroups.Remove(item);
                    return Unit.Default;
                })
                .Subscribe();


            //this.WhenAnyValue(x => x.SelectedItem)
            //    .Where(item => item != null)
            //    .Subscribe(item =>
            //    {
            //        NavigateTo(Services.NavigationDestinations.AddUserItemView,
            //             new UserItemDetailsViewModelNavigationArgs() { ItemToModify = item.Item });
            //    });
            #endregion

        }



        #region Commands

        public ReactiveCommand<Unit,Unit> AddNewItem { get; set; }

        public ReactiveCommand<Unit,Unit> RefreshItemList { get; set; }

        #endregion



        #region Properties


        public ShoppingList ShoppingList
        {
            get { return _shoppingList; }
            set { this.RaiseAndSetIfChanged(ref _shoppingList, value); }
        }

        ShoppingListItemViewModel _SelectedItem;
        public ShoppingListItemViewModel SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedItem, value);
            }
        }

        public AlphaKeyGroupList<ShoppingListItemViewModel> ItemGroups
        {
            get
            {
                return _ItemGroups;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _ItemGroups, value);
            }
        }

        ItemList _SelectedItemList;
        public ItemList SelectedItemList
        {
            get { return _SelectedItemList; }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedItemList, value);
            }
        }


        public ReactiveList<ShoppingListItemViewModel> Items
        {
            get;
            private set;
        }
        public ReactiveCommand<Unit,Unit> CompleteShopTheList { get; private set; }

#endregion
    }
}
