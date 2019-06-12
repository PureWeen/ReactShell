using ReactiveUI;
using SmartHotel.Clients.Core.Services;

namespace SmartHotel.Clients.Core.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected CustomNavigationServices CustomNavigationServices { get; } = new CustomNavigationServices();
    }
}