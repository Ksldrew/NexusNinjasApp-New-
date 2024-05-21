using NexusNinjasApplication;
using Microsoft.Maui.Controls;
using NexusNinjasApplication.Chatbox;
namespace NexusNinjasApplication
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the loading page as the main page initially
            MainPage = new NavigationPage(new LoadingPage());

            MeeageViewModel viewModel = new MeeageViewModel();

        }
    }
}