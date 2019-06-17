using ReactiveUI;
using Shopanizer.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Reactive.Linq;
using System.Reactive;
using Shopanizer.DataObjects;

namespace Shopanizer.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, ISupportsActivation,  IRoutableViewModel
    {


        static int ActivePages = 0;
        static object NavigationParameterContainer { get; set; }
        public object NavigationParameter { get; set; }

        Lazy<AppService> _appService;
        Lazy<INavigationService> _navigationService;


        protected AppService AppService { get { return _appService.Value; } }
        protected INavigationService NavigationService { get { return _navigationService.Value; } }


        Subject<bool> _onActivated;

        public ViewModelActivator Activator
        {

            get;
            private set;
        }

        public ViewModelBase(IScreen hostScreen = null)
        {

            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            Activator = new ViewModelActivator();
            _appService = new Lazy<AppService>(() => Locator.Current.GetService<AppService>());
            _navigationService = new Lazy<INavigationService>(() => Locator.Current.GetService<INavigationService>());


            _onActivated = new Subject<bool>();

            this.WhenActivated(d =>
            {
                if(NavigationParameterContainer != null)
                {
                    NavigationParameter = NavigationParameterContainer;
                    NavigationParameterContainer = null;
                }

                ActivePages++;
                _onActivated.OnNext(true);

                d(Disposable.Create(() =>
                {
                    ActivePages--;
                    _onActivated.OnNext(false);
                }));
            });
        }


		protected ReactiveCommand<Unit, T> CreateAsyncObservableCommand<T>(
            Func<Unit, IObservable<T>> executeAsync, 
            IObservable<bool> canExecute = null)
        {
            canExecute = canExecute ?? Observable.Return(true);
			var returnValue = ReactiveCommand.CreateFromObservable<Unit, T>(executeAsync, canExecute);

#if DEBUG
            returnValue.ThrownExceptions.Subscribe(OnThrownCommandException);
#endif

            returnValue.ThrownExceptions.Subscribe(OnError);
            return returnValue;
        }

        protected void OnThrownCommandException(Exception obj)
        {
            throw obj;
        }

        protected void OnError(Exception obj)
        {

            this.LogException(String.Format("VM:{0}, UserId:{1}, AppVersion:{2}", this.GetType().Name, UserId)
                , obj);

        }


        public string UserId
        {
            get;
            private set;
        }

        public IObservable<Unit> OnReActivated
        {
            get
            {
                return _onActivated.Where(x => x).Skip(1)
                   .Select(_ => Unit.Default)
                    .Log(this);
            }
        }

        public IObservable<Unit> OnFirstLoaded
        {
            get
            {
                return _onActivated.Where(x => x).Take(1)
                    .Select(_=> Unit.Default)
                .Log(this);
            }
        }

        protected AppBootStrapper ApplicationState
        {
            get
            {
                var returnValue = RxApp.SuspensionHost.GetAppState<AppBootStrapper>();
                if (returnValue == null)
                    throw new Exception("Application state is null");

                return returnValue;
            }
        }

        public void NavigateTo(NavigationDestinations destination)
        {

            NavigateTo(destination, null);

        }

        public void NavigateTo(NavigationDestinations destination, object args, bool keepMeInNavigationStack = true)
        {
            NavigationParameterContainer = args;
            NavigationService.Navigate(destination, keepMeInNavigationStack);
        }

        public bool IsDesigner
        {
            get
            {
                return ModeDetector.InDesignMode();
            }
        }

        public virtual string UrlPathSegment
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public IScreen HostScreen
        {
            get;
            private set;
        }
    }
}
