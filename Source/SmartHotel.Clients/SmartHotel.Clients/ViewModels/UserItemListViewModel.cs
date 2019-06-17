using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using Shopanizer.DataObjects;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections;
using System.Linq;
using System.Reactive.Subjects;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Splat;
using RxUI.XamForms.Controls;
using ReactiveUI.Legacy;

namespace Shopanizer.ViewModels
{
    public class UserItemListViewModel : ViewModelBase
    {

        ReactiveList<GroupedUserItemListItemViewModel> _ItemGroupsSource;
        IReactiveCollection<GroupedUserItemListItemViewModel> _ItemGroups;


        IReactiveList<KeyValuePair<string, ItemList>> _ItemLists;

        ObservableReactiveList<GroupedUserItemListItemViewModel> _HackSourceList;
        public ObservableReactiveList<GroupedUserItemListItemViewModel> HackSourceList
        {
            get
            {
                return _HackSourceList;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _HackSourceList, value);
            }
        }


        public UserItemListViewModel() : this(null)
        {

        }

        public UserItemListViewModel(IScreen hostScreen) : base(hostScreen)
        {
            _ItemGroupsSource = new ReactiveList<GroupedUserItemListItemViewModel>(scheduler: System.Reactive.Concurrency.ImmediateScheduler.Instance);
            _ItemGroups = _ItemGroupsSource.CreateDerivedCollection(x => x, filter: x => x.HasItems, orderer:(x,y) => x.GroupName.CompareTo(y.GroupName), scheduler: System.Reactive.Concurrency.ImmediateScheduler.Instance);
            ItemGroupsSource.ChangeTrackingEnabled = true;
            _ItemGroups.ChangeTrackingEnabled = true;

           

            ItemLists = new ReactiveList<KeyValuePair<string, ItemList>>();

            Func<UserItemListItemViewModel, bool> filter = (item) =>
            {
                bool returnValue = (SelectedItemList == null || item.ItemLists.Contains(GetItemList()));

                if(returnValue && IsShopping)
                {
                    if(item == null)
                    {
                        throw new ArgumentNullException("item");
                    }

                    returnValue = item.Need;
                }

                return returnValue;
            };


            Subject<Unit> signalReset = new Subject<Unit>();


            var signalThings = this.WhenAnyValue(x => x.IsShopping).Skip(1).UnitDefault()
                                .Merge(this.WhenAnyValue(x => x.SelectedItemList).Skip(1).UnitDefault());

            var closureHackSourceList = new ObservableReactiveList<GroupedUserItemListItemViewModel>(ItemGroups, true);
            HackSourceList = closureHackSourceList;

            signalThings
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ =>
                {
                    HackSourceList = null;
                    return Unit.Default;
                })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(_ =>
                {
                    signalReset.OnNext(Unit.Default);
                    return Unit.Default;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ =>
                {
                    this.Log().Debug("setting HackSourceList");
                    HackSourceList = closureHackSourceList;
                    this.Log().Debug("set HackSourceList");
                    return Unit.Default;
                })
                .Subscribe();

           
            #region commands

            var loadItemLists =
                Observable.Defer(() => AppService.GetItemListsSelectList()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(itemLists =>
                {
                    var activeList = ApplicationState.SelectedList;
                    itemLists.ToList().ForEach(x =>
                        {
                            if (!ItemLists.Any(k => k.Key == x.Key))
                                ItemLists.Add(x);
                        }
                    );

                    if (activeList != null)
                    {
                        SelectedItemList = itemLists
                            .Select(x => x)
                            .FirstOrDefault(x => activeList.Equals(x.Value));
                    }

                    return Unit.Default;
                }))
               .LoggedCatchExt (this, next: (Exception e)=> Observable.Return(Unit.Default), message: "loadItemLists");



            ObservableReactiveList<GroupedUserItemListItemViewModel>
                winrtHackHolder = null;


            var loadItems = Observable.Defer(() => AppService.GetItems()
                .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(il =>
                    {

                        var tempList = HackSourceList ?? winrtHackHolder;
                        HackSourceList = null;
                        SelectedItem = null;


                        //HACK:SHane look at delete messages maybe using pub sub

                        foreach (var group in _ItemGroupsSource)
                        {
                            foreach (var item in group.ToList())
                            {
                                if (!il.Any(x => x.Id == item.Id))
                                {
                                    group.Remove(item);
                                }
                            }
                        }

                        foreach (var i in il)
                        {   
                            var localItem = i;

                            UserItemListItemViewModel existingItemVm = null;
                            GroupedUserItemListItemViewModel existingGroupVM = null;
                            foreach(var group in _ItemGroupsSource)
                            {
                                existingItemVm = group.GetItemVM(i);
                                if (existingItemVm != null)
                                {
                                    existingGroupVM = group;
                                    break;
                                }
                            }
                                

                            if(existingItemVm != null)
                            {
                                existingItemVm.SetupItem(localItem);

								string aisleName = String.Empty;

								if(i.Aisle != null)
									aisleName = i.Aisle.Name;

                                //Locate the group I should be in
                                var theGroup = _ItemGroupsSource.FirstOrDefault(x => x.GroupName == aisleName);
                                if (theGroup == null)
                                {
                                    theGroup = new GroupedUserItemListItemViewModel(aisleName, aisleName, filter, signalReset);
                                    _ItemGroupsSource.Add(theGroup);
                                }

                                //My group changed :-[
                                if (!theGroup.Contains(existingItemVm.Item))
                                {
                                    

                                    if(existingGroupVM != null && 
                                        existingGroupVM.GroupName != aisleName)
                                    {
                                        existingGroupVM.Remove(existingItemVm);
                                    }

                                    theGroup.Add(existingItemVm);
                                }
                            }
                            else
                            {

                                var itemVm = new UserItemListItemViewModel(i, AppService.SaveItem(localItem));

								string aisleName = String.Empty;

								if(i.Aisle != null)
									aisleName = i.Aisle.Name;
							
                                var theGroup = _ItemGroupsSource.FirstOrDefault(x => x.GroupName == aisleName);

                                if (theGroup == null)
                                {
                                    theGroup = new GroupedUserItemListItemViewModel(aisleName, aisleName, filter, signalReset);
                                    _ItemGroupsSource.Add(theGroup);
                                }

                                theGroup.Add(itemVm);
                            }

                        }

                        HackSourceList = tempList;
                        return Unit.Default;
                    }))
               .LoggedCatchExt(this, next: (Exception e) => Observable.Return(Unit.Default), message: "loadItems");


            RefreshItemList = ReactiveCommand.CreateFromObservable<Unit,Unit>(_ =>
                {
                    return Observable.Zip(loadItemLists, loadItems, (x, y) => Unit.Default);
                }
            );



			AddNewItem = ReactiveCommand.CreateFromObservable<Unit,Unit>(_ =>
            {
                NavigateTo(Services.NavigationDestinations.AddUserItemView, 
                    new UserItemDetailsViewModelNavigationArgs() { SelectedItemList = GetItemList() });

                return Observable.Empty<Unit>();

            }, canExecute:Observable.Return(true));

			ShopTheList = ReactiveCommand.CreateFromObservable<Unit,Unit>(
                
                _ =>
                {
                    return AppService.StartShoppingTrip(GetItemList())
                    .Select(shoppingList =>
                    {
                        if (!shoppingList.IsError)
                        {
                            NavigateTo(Services.NavigationDestinations.ShoppingListView, null, keepMeInNavigationStack: false);
                        }

                       return Unit.Default;
                    });                    
                },
				this.WhenAny(x=> x.SelectedItemList, selected=> selected.Value != null)
            );

            #endregion


            #region eventing and things

            this.WhenAnyValue(x => x.SelectedItemList).Skip(1)
                .Select(item =>
                RxApp.SuspensionHost.GetAppState<AppBootStrapper>().SelectedList = GetItemList())
                .LoggedCatchExt(this, (Exception exc)=> Observable.Empty<ItemList>())
                .Subscribe();

            //https://bugzilla.xamarin.com/show_bug.cgi?id=35026            
            this.WhenNavigatingFromObservable()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    winrtHackHolder = HackSourceList;
                    HackSourceList = null;
                });


            this.OnFirstLoaded
                .SelectMany(_ => AppService.TryToAuthenticate())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(user =>
                {
                    if (user == null)
                    { 
                        NavigateTo(Services.NavigationDestinations.LoginView);
                    }
                    return user;
                })
                 .Where(user => user != null).UnitDefault()
                .InvokeCommand(RefreshItemList);

            this.OnReActivated
                .SelectMany(_ => AppService.TryToAuthenticate())
                .Where(user => user != null).UnitDefault()
                .ObserveOn(RxApp.MainThreadScheduler)
                .InvokeCommand(RefreshItemList);
               
                   


            this.WhenAnyValue(x => x.SelectedItem)
                .Skip(1)
                .Where(item => item != null)
                .Subscribe(item =>
                {
                    NavigateTo(Services.NavigationDestinations.AddUserItemView,
                         new UserItemDetailsViewModelNavigationArgs() { ItemToModify = item.Item });

                });
            #endregion
        }


        public override string UrlPathSegment
        {
            get
            {
                return "Your Items";
            }
        }


        #region Commands

        ReactiveCommand<Unit, Unit> _AddNewItem;
        public ReactiveCommand<Unit,Unit> AddNewItem { get { return _AddNewItem; } set { this.RaiseAndSetIfChanged(ref _AddNewItem, value); } }

        public ReactiveCommand<Unit,Unit> RefreshItemList { get; set; }

        #endregion



        #region Properties


        bool _IsShopping;
        public bool IsShopping
        {
            get
            {
                return _IsShopping;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _IsShopping, value);
            }
        }

        UserItemListItemViewModel _SelectedItem;
        public UserItemListItemViewModel SelectedItem
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

        public ReactiveList<GroupedUserItemListItemViewModel> ItemGroupsSource
        {
            get
            {
                return _ItemGroupsSource;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref _ItemGroupsSource, value);
            }
        }

        public IReactiveCollection<GroupedUserItemListItemViewModel> ItemGroups
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

        public IReactiveList<KeyValuePair<string, ItemList>> ItemLists
        {
            get
            {
                return _ItemLists;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _ItemLists, value);
            }
        }

        

        ItemList GetItemList()
        {
            if (SelectedItemList.HasValue)
                return SelectedItemList.Value.Value;

            return null;
        }

        KeyValuePair<string, ItemList>? _SelectedItemList;
        public KeyValuePair<string, ItemList>? SelectedItemList
        {
            get { return _SelectedItemList; }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedItemList, value);
            }
        }

        public ReactiveCommand<Unit,Unit> ShopTheList { get; private set; }
        #endregion






    
        public class UserItemListItemViewModel : ReactiveObject, IComparable<UserItemListItemViewModel>
        {
            bool _Need;

            public Item Item
            {
                get;
                set;
            }

            public void SetupItem(Item item)
            {
                Name = item.Name;
                Aisle = item.Aisle;
                Need = item.Need;
                Item = item;
                Id = item.Id;


                
                if (ItemLists == null ||
                    ItemLists.Any(x => !item.ItemLists.Contains(x)))
                {
                    ItemLists = new List<ItemList>(item.ItemLists);
                }
            }

            public UserItemListItemViewModel(Item item, IObservable<Item> needChanged)
            {
                SetupItem(item);

                this.WhenAnyValue(x => x.Need)
                    .Where(changedTo => Item.Need != changedTo)
                    .SelectMany(changedTo =>
                    {
                        Item.Need = Need;
                        return needChanged;
                    })
                    .Subscribe();
            }

            bool _IncludeInList;

            public bool IncludeInList
            {
                get
                {
                    return _IncludeInList;
                }
                set
                {
                    this.RaiseAndSetIfChanged(ref _IncludeInList, value);
                }
            }


            public string Id { get; set; }


            public String Name { get; set; }
            public Aisle Aisle { get; set; }
            public bool Need
            {
                get { return _Need; }
                set { this.RaiseAndSetIfChanged(ref _Need, value); }
            }

            List<ItemList> _ItemLists;
            public List<ItemList> ItemLists { get { return _ItemLists; }
                set
                {
                    this.RaiseAndSetIfChanged(ref _ItemLists, value);
                }
            }



            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                return ((UserItemListItemViewModel)obj).Id.Equals(Id);
            }

            public int CompareTo(UserItemListItemViewModel other)
            {
                return Name.CompareTo(other.Name);
            }
        }
    }
}