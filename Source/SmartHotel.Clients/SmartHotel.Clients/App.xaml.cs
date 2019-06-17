using Shopanizer;
using System.Threading.Tasks;
using Xamarin.Forms;
//TODO SHANE can we make this work?
//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SmartHotel.Clients
{
    public partial class App : Application
    {
        public App()
        {

            InitializeComponent();

            MainPage = new AppShell();
        }

    }
}
