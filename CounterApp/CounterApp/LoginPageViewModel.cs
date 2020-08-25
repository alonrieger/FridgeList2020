using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Threading.Tasks;

//using Microsoft.Azure.Devices;
using System.Text;
using System.Dynamic;
using ZXing.Net.Mobile.Forms;
using System.Collections.ObjectModel;

namespace CounterApp
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public String Username;
        public String Password;

        public String username
        {
            get => Username;
            set
            {
                Username = value;
                var args = new PropertyChangedEventArgs(nameof(username));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public String password
        {
            get => Password;
            set
            {
                Password = value;
                var args = new PropertyChangedEventArgs(nameof(password));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public ICommand LoginCmd { get; }

        public LoginPageViewModel()
        {
            LoginCmd = new Command(LoginCd);
        }
       
        async Task<bool> username_already_exists(String uName)
        {
            List<String> users_column = await tableFunctions.get_column("UsersTable", "RowKey");
            return users_column.Contains(uName);            
        }

        async Task<bool> password_incorrect(String pWord,String uName)
        {
            string password = await tableFunctions.get_value("UsersTable", "Password", uName);
            return !(String.Equals(password, pWord));
        }


        private async void LoginCd()
        {
            if ((JoinFridgePageViewModel.invalid_input(password)) || (JoinFridgePageViewModel.invalid_input(username)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Password or Username fields are empty", "OK");
            }
            else if (!(await username_already_exists(username)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Username does not exist", "OK");
            }
            else if (await password_incorrect(password, username))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Password is incorrect", "OK");
            }
            else
            {
                Application.Current.Properties["username"] = username;
                var AccountVM = new AccountSettingsPageViewModel();
                var AccountSettingsXaml = new AccountSettingsPage();
                AccountSettingsXaml.BindingContext = AccountVM;
                await Application.Current.MainPage.Navigation.PushAsync(AccountSettingsXaml);
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
            }
            password = null;
            username = null;
        }
    }
}
