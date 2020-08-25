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
    public class NewFridgePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public String FridgeName;
       
        public String fridgeName
        {
            get => FridgeName;
            set
            {
                FridgeName = value;
                var args = new PropertyChangedEventArgs(nameof(fridgeName));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public ICommand RegisterFridgeCommand { get; }

        public NewFridgePageViewModel()
        {
            RegisterFridgeCommand = new Command(RegFridgeCommand);
        }

        async Task<bool> add_fridge(String FName, String Uname)
        {
            bool result = await tableFunctions.new_tables(FName, Uname);
            return result;
        }

        async Task<bool> fridgename_already_exists(String FName)
        {
            List<String> fridgeNames = await tableFunctions.get_column("FridgesTable", "RowKey");
            return fridgeNames.Contains(FName);
        }

        bool alphaNumeric(String name)
        {
            foreach (char c in name)
            {
                if (!Char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        private async void RegFridgeCommand()
        {
            if (JoinFridgePageViewModel.invalid_input(FridgeName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fridge name field is empty", "OK");
            }
            else if (await fridgename_already_exists(FridgeName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fridge name already exists", "OK");
            }
            else if (! alphaNumeric(FridgeName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fridge name field must contain only alpha-numeric characters", "OK");
            }
            else
            {
                bool wasLoggedIn = Application.Current.Properties.ContainsKey("IsLoggedIn") ? Convert.ToBoolean(Application.Current.Properties["IsLoggedIn"]) : false;
                Application.Current.Properties["fridgename"] = FridgeName;
                Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;
                bool add = await add_fridge(FridgeName, Convert.ToString(Application.Current.Properties["username"]));

                var MainVM = new MainViewModel();
                var MainXaml = new MainPage();
                MainXaml.BindingContext = MainVM;
                await Application.Current.MainPage.Navigation.PushAsync(MainXaml);
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
            }
            FridgeName = null;
        }
    }
}
