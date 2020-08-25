using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Push;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CounterApp
{
    public partial class App : Application
    {
        
        public App()
        {
            InitializeComponent();
            
            bool isLoggedIn = Current.Properties.ContainsKey("IsLoggedIn") ? Convert.ToBoolean(Current.Properties["IsLoggedIn"]) : false;
            if (isLoggedIn)
            {
                MainPage = new NavigationPage(new MainPage());
                Application.Current.Properties["WasLoggedIn"] = Boolean.TrueString;
            }
            else
            {
                MainPage = new NavigationPage(new SignUpPage());
                Application.Current.Properties["WasLoggedIn"] = Boolean.FalseString;
            }
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {
           
        }
    }
}
