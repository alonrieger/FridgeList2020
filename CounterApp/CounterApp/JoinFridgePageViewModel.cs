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
    public class JoinFridgePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand JoinFridgeCommand { get; }

        public String FridgeName;
        public String FridgeID;

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

        public String fridgeID
        {
            get => FridgeID;
            set
            {
                FridgeID = value;
                var args = new PropertyChangedEventArgs(nameof(fridgeID));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public JoinFridgePageViewModel()
        {
            JoinFridgeCommand = new Command(JoinFridgeCmd);
        }

        async Task<bool> fridgename_already_exists(String FName)
        {
            return await tableFunctions.table_exists(FName+ "Users");
        }

        async Task<bool> fridgeid_incorrect(String FID,String FName)
        {
            String fridgeID = await tableFunctions.get_value("FridgesTable","Id" ,FName);
            return !string.Equals(FID, fridgeID);
        }

        async void add_to_table(String FName, String Uname)
        {
            List<String> arg = new List<String>();
            arg.Add(Uname);
            arg.Add("");
            await tableFunctions.add_to_table("usersInFridge", FName + "Users", arg);
        }

        public static bool invalid_input(String input)
        {
            return ((input == null) || (string.Compare(input, "") == 0) || (string.Compare(input, "\n") == 0));
        } 

        private async void JoinFridgeCmd()
        {
            if ((invalid_input(FridgeID)) || (invalid_input(FridgeName)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fridge ID or name fields are empty", "OK");
            }
            else if (!(await fridgename_already_exists(FridgeName)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fridge name does not exist", "OK");
            }
            else if (await fridgeid_incorrect(FridgeID, FridgeName))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fridge ID is incorrect", "OK");
            }
            else
            {
                add_to_table(FridgeName, Convert.ToString(Application.Current.Properties["username"]));
                Application.Current.Properties["fridgename"] = FridgeName;
                Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;
                var MainVM = new MainViewModel();
                    var MainXaml = new MainPage();
                    MainXaml.BindingContext = MainVM;
                    await Application.Current.MainPage.Navigation.PushAsync(MainXaml);
                    Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
                    Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack.ElementAt(Application.Current.MainPage.Navigation.NavigationStack.Count - 2));
            }
            fridgeID = "";
            fridgeName = "";
        }
    }
}
