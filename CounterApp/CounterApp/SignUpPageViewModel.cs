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
    
    public class SignUpPageViewModel : INotifyPropertyChanged
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
        public ICommand RegisterCommand { get; }

        public ICommand LoginCommand { get; }

        public SignUpPageViewModel()
        {
            RegisterCommand = new Command(RegCommand);

            LoginCommand = new Command(LogCommand);
        }

        async Task<bool> username_already_exists(String uName)
        {
            List<String> users_column = await tableFunctions.get_column("UsersTable", "RowKey");
            return users_column.Contains(uName);
        }

        async Task<bool> save_password_and_username(String pWord, String uName)
        {
            List<String> arg = new List<String>();
            arg.Add(uName);
            arg.Add(pWord);
            return await tableFunctions.add_to_table("allUsers", "UsersTable", arg);
        }

        private async void RegCommand()
        {
            Application.Current.Properties["IsLoggedIn"] = Boolean.FalseString;
            if((JoinFridgePageViewModel.invalid_input(password)) || (JoinFridgePageViewModel.invalid_input(username)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Password or Username fields are empty", "OK");
            }
            else if (await username_already_exists(username))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Username already exists", "OK");
            }
            else
            {
                bool success = await save_password_and_username(password,username);  
                Application.Current.Properties["username"] = username;
                var AccountVM = new AccountSettingsPageViewModel();
                var AccountSettingsXaml = new AccountSettingsPage();
                AccountSettingsXaml.BindingContext = AccountVM;
                await Application.Current.MainPage.Navigation.PushAsync(AccountSettingsXaml);
            }
            password = null;
            username = null;
        }

        private async void LogCommand()
        {
            Application.Current.Properties["IsLoggedIn"] = Boolean.FalseString;
            var LoginVM = new LoginPageViewModel();
            var LoginXaml = new LoginPage();
            LoginXaml.BindingContext = LoginVM;
            await Application.Current.MainPage.Navigation.PushAsync(LoginXaml);
        }

    }
}



