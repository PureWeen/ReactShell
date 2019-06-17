using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI;
using System.Reactive;
using Shopanizer.DataObjects;
using System.Drawing;

namespace Shopanizer.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {

        public LoginViewModel() : this(null)
        {
        }

        public LoginViewModel(IScreen hostScreen) : base(hostScreen)
        {
            LoginMessage = "Loading Please Hold.....";

            var tryToAuthenticate = CreateAsyncObservableCommand<Unit>(_ =>
                    AppService
                        .TryToAuthenticate()
#if DEBUG
                       // .Delay(TimeSpan.FromSeconds(5))
#endif
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .SelectMany(result =>
                        {
                            if (result != null)
                            {
                                //HACK:This doesn't work in XAM forms
                                NavigateTo(Services.NavigationDestinations.UserItemListView, null, false);

                                //NavigationService.NavigateAndReset(Services.NavigationDestinations.UserItemListView);
                              //  return NavigationService.GoBack();
                                
                            }
                            else
                            {
                                LoginMessage = "Please Enter Credentials";
                            }

                            return Observable.Return( Unit.Default);
                        })
                );


            var canExecute = 
				this.WhenAnyValue(x => x.UserName, x => x.Password, (u, p) =>
					!String.IsNullOrWhiteSpace(u) && !String.IsNullOrWhiteSpace(p));

			canExecute =	Observable.CombineLatest (
				canExecute,
				tryToAuthenticate.CanExecute,
				(x, y) => x & y
			);
				


            Register = CreateAsyncObservableCommand<Unit>(_ =>
            {
                return AppService.UsernameAndPasswordRegister(UserName, Password)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SelectMany(result =>
                    {
                        if(result.IsError)
                        {
                            LoginMessage = result.ServerErrorMessage;
                            return Observable.Return(Unit.Default);
                        }

                        return Login.Execute();
                    });

            }, canExecute);
            Login = CreateAsyncObservableCommand<Unit>(_ =>
            {
                return AppService.UsernameAndPasswordAuthenticate(UserName, Password)
                    .SelectMany(result =>
                    {
                        if (result)
                        {
                            return AppService.GetUser();
                        }

                        return Observable.Return(new User() { ServerErrorMessage = "FAIL" });
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(result =>
                    {
                        if (result.IsError)
                        {
                            LoginMessage = result.ServerErrorMessage;
                        }
                        else
                        {
                            LoginMessage = "Success Navigating places";
                            if (result.CurrentShoppingTrip == null)
                            {
                                NavigateTo(Services.NavigationDestinations.UserItemListView, null, false);
                            }
                            else
                            {
                                NavigateTo(Services.NavigationDestinations.ShoppingListView, null, false);
                            }
                        }

                        return Unit.Default;
                    });

            }, canExecute);


            this.OnFirstLoaded
                .SelectMany(_=> tryToAuthenticate.Execute())
                .Subscribe();

            Random somethingRandom = new Random();

            tryToAuthenticate 
                .IsExecuting
                .CombineLatest(Login.IsExecuting, (x,y) => x || y)
                .Select(yes =>
                {
                    if(!yes)
                    {
                        Progress = 0;
                        return Observable.Never<Unit>();
                    }

                    return Observable.Interval(TimeSpan.FromMilliseconds(300))
                        .Select(_ => Unit.Default);
                })
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    Progress += (1.0 / somethingRandom.Next(1, 100));
                    if (Progress > 1)
                    {
                        Progress = 0;
                    }

                });

            this.WhenAnyValue(x => x.Progress)
                .Subscribe(progress =>
                {
                    ProgressColor =  Xamarin.Forms.Color.FromRgb(
                        Convert.ToInt32(255 * Progress), Convert.ToInt32(255 - (255 * Progress)), 14);
                });
        }

        Xamarin.Forms.Color _progressColor;
        public Xamarin.Forms.Color ProgressColor
        {
            get
            {
                return _progressColor;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _progressColor, value);
            }
        }

        double _progress;
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _progress, value);
            }
        }

        string _LoginMessage;
        public string LoginMessage
        {
            get
            {
                return _LoginMessage;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _LoginMessage, value);
            }
        }

        string _UserName;
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _UserName, value);
            }
        }

        string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _Password, value);
            }
        }


        public ReactiveCommand<Unit,Unit> Register { get; private set; }
        public ReactiveCommand<Unit,Unit> Login { get; private set; }

        public override string UrlPathSegment
        {
            get
            {
                return "Login Page";
            }
        }
    }
}
