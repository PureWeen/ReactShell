using ReactiveUI;
using ReactiveUI.Legacy;
using Shopanizer.DataObjects;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Shopanizer.ViewModels
{
    public class GroupedUserItemListItemViewModel : ReactiveObject, IEnumerable<UserItemListViewModel.UserItemListItemViewModel>, INotifyCollectionChanged, IEnableLogger, IComparable<GroupedUserItemListItemViewModel>
    {
        public ReactiveList<UserItemListViewModel.UserItemListItemViewModel> _InternalListSource = new ReactiveList<UserItemListViewModel.UserItemListItemViewModel>();
        public IReactiveDerivedList<UserItemListViewModel.UserItemListItemViewModel> _InternalList = null;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        Func<UserItemListViewModel.UserItemListItemViewModel, bool> _filter;

        public GroupedUserItemListItemViewModel(String Name, String ShortName, Func<UserItemListViewModel.UserItemListItemViewModel, bool> filter, IObservable<Unit> signalReset)
        {
            this.GroupName = Name;
            this.ShortName = ShortName;

            _filter = filter;

            _InternalList = _InternalListSource.CreateDerivedCollection(
                x => x, filter: filter,
                orderer: (x, y) => x.Name.CompareTo(y.Name),
                signalReset: signalReset);

            _InternalList.ChangeTrackingEnabled = true;
            _InternalListSource.ChangeTrackingEnabled = true;

            _InternalList.IsEmptyChanged
                .Subscribe(isEmpty => HasItems = !isEmpty);
        }


        bool _hasItems;
        //ObservableAsPropertyHelper<bool> _hasItems;
        public bool HasItems
        {
            get
            {
                return _hasItems;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _hasItems, value);
            }
        }

        string _name;
        public String GroupName
        {
            get
            {
                return _name;
            }
            private set
            {

                _name = value;
            }
        }


        string _shortName;

        public String ShortName
        {
            get
            {
                return _shortName;
            }
            private set
            {
                _shortName = value;
            }
        }


        public IEnumerator<UserItemListViewModel.UserItemListItemViewModel> GetEnumerator()
        {
            return _InternalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _InternalList.GetEnumerator();
        }


        internal UserItemListViewModel.UserItemListItemViewModel GetItemVM(Item item)
        {
            return _InternalListSource.FirstOrDefault(x => x.Id == item.Id);
        }

        internal bool Contains(Item item)
        {
            return _InternalListSource.Any(x => x.Id == item.Id);
        }

        internal void Remove(UserItemListViewModel.UserItemListItemViewModel item)
        {
            _InternalListSource.Remove(item);
        }

        internal void Add(UserItemListViewModel.UserItemListItemViewModel item)
        {
            item.IncludeInList = _filter(item);
            _InternalListSource.Add(item);
        }

        public int CompareTo(GroupedUserItemListItemViewModel other)
        {
            if (other == null)
                return this.GroupName.CompareTo(null);

            return this.GroupName.CompareTo(other.GroupName);
        }

        public override bool Equals(object obj)
        {
            var equalsTo = obj as GroupedUserItemListItemViewModel;

            if (equalsTo == null)
                return false;

            return equalsTo.GroupName.Equals(GroupName);
        }

        public override int GetHashCode()
        {
            return GroupName.GetHashCode();
        }
    }

}
