using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HP.SecureStorage {
    public sealed partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();
            View.Initialize();
        }

        internal MainPageView View => DataContext as MainPageView;
    }
}
