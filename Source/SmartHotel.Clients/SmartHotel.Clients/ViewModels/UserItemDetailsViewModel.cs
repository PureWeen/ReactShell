using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using Splat;
using Shopanizer.DataObjects;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI.Legacy;

namespace Shopanizer.ViewModels
{
    public class UserItemDetailsViewModel : ViewModelBase
    {
        Aisle _SelectedAisle;
        ItemList _SelectedItemList;

        bool isAddMode = false;

        int ListCompare<TValue>(
            KeyValuePair<string, TValue> item1,
            KeyValuePair<string, TValue> item2) where TValue : class
        {
            if (item1.Value == item2.Value)
                return 0;

            if (item2.Value == null)
                return 1;

            if (item1.Value == null)
                return -1;

            return item1.Key.CompareTo(item2.Key);

        }


        public override string UrlPathSegment
        {
            get
            {
                if (!isAddMode)
                    return "Edit Item";

                return "Add New Item";
            }
        }
        public UserItemDetailsViewModel()
        {
            Aisles = new ReactiveList<KeyValuePair<string, Aisle>>();
            AislesDataSource = Aisles.CreateDerivedCollection(x => x, orderer: (x, y) =>
                 ListCompare(x, y));

            ItemLists = new ReactiveList<KeyValuePair<string, ItemList>>();
            ItemListsSource = ItemLists.CreateDerivedCollection(x => x, orderer: (x, y) =>
                ListCompare(x, y));



            //Load Aisles data
            var loadAisles = AppService.GetAisles()
                .ObserveOn(RxApp.MainThreadScheduler)
                        .Select(aisles =>
                        {
                            aisles.ToSelectItemList(aisle => aisle.Name, "No Aisle", Aisles);
                            return Unit.Default;
                        });

            var loadItemLists = AppService.GetItemListsSelectList()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(itemLists =>
                {
                    itemLists.ToList().ForEach(x => ItemLists.Add(x));
                    return Unit.Default;
                });

            OnFirstLoaded
                .SelectMany(_=> loadAisles)
                .SelectMany(_ => loadItemLists)
                .Subscribe(_=>
                {
                    var navParam = this.GetNavigationParameter<UserItemDetailsViewModelNavigationArgs>();

                    if (navParam != null)
                    {
                        SelectedItemList = navParam.SelectedItemList;

                        SelectedItem = navParam.ItemToModify;

                        if(SelectedItem != null)
                        {
                            Name = SelectedItem.Name;
                            var itemList = SelectedItem.ItemLists.FirstOrDefault();                            
                            var selectedAisle = SelectedItem.Aisle;

                            if(itemList != null)
                            {
                                SelectedItemList = ItemLists.FirstOrDefault(x => itemList.Equals(x.Value)).Value;
                            }

                            if(selectedAisle != null)
                            {
                                SelectedAisle = Aisles.FirstOrDefault(x => selectedAisle.Equals(x.Value)).Value;
                            }
                            Need = SelectedItem.Need;
                        }
                        else
                        {
                            isAddMode = true;
                            Need = true;
                        }
                    }


                    var deleteObservable = NavigationService.DisplayAlert("Confirm", "Are you sure you want to Delete?", "Yes Delete", "No")
                    .Where(x => x)
                    .SelectMany(__ => AppService.DeleteItem(SelectedItem))
                    .Where(x => x)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SelectMany(__ => NavigationService.GoBack())
                    .UnitDefault();
                   

                    DeleteItem = ReactiveCommand.CreateFromObservable<Unit>(
                        () => deleteObservable,
                        canExecute:Observable.Return(!isAddMode));


                });


			SaveItem = ReactiveCommand.CreateFromObservable<Unit,Unit>(
                
                _ =>
                {
                    var addME = SelectedItem ??
                        new Item(UserId, Name.Trim());

                    addME.Name = Name;
                    addME.Need = Need;

                    //TODO right now AIsle's and ItemLists don't persist if no one is using them
                    if(SelectedAisle == null && !String.IsNullOrWhiteSpace(NewAisle))
                    {
                        var newAisle = new Aisle() { Name = NewAisle };
                        SelectedAisle = Aisles.FirstOrDefault(x => newAisle.Equals(x.Value)).Value;

                        if(SelectedAisle == null)
                        {
                            Aisles.Add(new KeyValuePair<string, Aisle>(NewAisle, addME.Aisle));
                            SelectedAisle = newAisle;
                        }

                        addME.Aisle = SelectedAisle;
                        NewAisle = string.Empty;
                    }
                    else
                    {
                        addME.Aisle = SelectedAisle;
                    }

                    addME.ItemLists.Clear();

                    if (SelectedItemList != null)
                    {
                        if (!addME.ItemLists.Contains(SelectedItemList))
                        {
                            addME.ItemLists.Add(SelectedItemList);
                        }
                    }
                    else if(!String.IsNullOrWhiteSpace(NewItemList))
                    {
                        ItemList newList = new ItemList() { Name = NewItemList };
                        SelectedItemList = ItemLists.FirstOrDefault(x => newList.Equals(x.Value)).Value;

                        if (SelectedItemList == null)
                        {
                            ItemLists.Add(new KeyValuePair<string, ItemList>(NewItemList, newList));
                            SelectedItemList = newList; 
                        }

                        addME.ItemLists.Add(SelectedItemList);
                        NewItemList = String.Empty;
                    }

                    Name = String.Empty;


                    return AppService.SaveItem(addME)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SelectMany(__ =>
                    {
                        if (!isAddMode)
                        {
                            return NavigationService.GoBack();
                        }

                        return Observable.Return( Unit.Default);
                    }
                    );
                },
				this.WhenAnyValue(x => x.Name).Select(name =>
				{
					return !String.IsNullOrWhiteSpace(Name);
				}));

            


            this.WhenAnyValue(x => x.NewItemList)
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => SelectedItemList = null);

            this.WhenAnyValue(x => x.NewAisle)
               .Where(x => !String.IsNullOrWhiteSpace(x))
               .ObserveOn(RxApp.MainThreadScheduler)
               .Subscribe(_ => SelectedAisle = null);

            this.WhenAnyValue(x => x.SelectedAisle)
              .Where(x => x != null)
              .ObserveOn(RxApp.MainThreadScheduler)
              .Subscribe(_ => NewAisle = String.Empty);

            this.WhenAnyValue(x => x.SelectedItemList)
               .Where(x => x != null)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Subscribe(_ => NewItemList = String.Empty);
        }

        ReactiveCommand<Unit, Unit> _DeleteItem;
        public ReactiveCommand<Unit,Unit> DeleteItem { get { return _DeleteItem; } set { this.RaiseAndSetIfChanged(ref _DeleteItem, value); } }

        public Item SelectedItem
        {
            get;
            private set;
        }


        string _NewItemList;

        public string NewItemList { get { return _NewItemList;  } set { this.RaiseAndSetIfChanged(ref _NewItemList, value); } }

        string _NewAisle;
        public string NewAisle { get { return _NewAisle; } set { this.RaiseAndSetIfChanged(ref _NewAisle, value); } }


        public IReactiveList<KeyValuePair<string, ItemList>> ItemLists { get;  set; }
        public IReactiveList<KeyValuePair<string, Aisle>> Aisles { get;  set; }

        KeyValuePair<string, Aisle>? _SelectedAisleKeyValuePair;
        public KeyValuePair<string, Aisle>? SelectedAisleKeyValuePair
        {
            get
            {
                return _SelectedAisleKeyValuePair;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _SelectedAisleKeyValuePair, value);

                if (value == null)
                    SelectedAisle = null;
                else
                    SelectedAisle = value.Value.Value;
            }
        }

        public Aisle SelectedAisle
        {
            get {
                return _SelectedAisle;
            }
             set {
                this.RaiseAndSetIfChanged(ref _SelectedAisle, value);
            } }

        public ItemList SelectedItemList { get { return _SelectedItemList; } set { this.RaiseAndSetIfChanged(ref _SelectedItemList, value); } }
        public ReactiveCommand<Unit,Unit> SaveItem { get;  set; }


        bool _Need;
        public bool Need { get { return _Need; } set { this.RaiseAndSetIfChanged(ref _Need, value); } }


        string _name;
        public string Name { get { return _name ?? String.Empty; } set { this.RaiseAndSetIfChanged(ref _name, value); } }

        public IReactiveDerivedList<KeyValuePair<string, Aisle>> AislesDataSource { get; private set; }
        public IReactiveDerivedList<KeyValuePair<string, ItemList>> ItemListsSource { get; private set; }
    }




    public class UserItemDetailsViewModelNavigationArgs
    {
        public ItemList SelectedItemList { get; set; }
        public Item ItemToModify { get; set; }
    }
}
