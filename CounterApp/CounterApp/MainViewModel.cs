using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;

using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System.Threading.Tasks;

//using Microsoft.Azure.Devices;
using System.Text;
using System.Dynamic;
using ZXing.Net.Mobile.Forms;

namespace CounterApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        HubConnection connection;
        static Microsoft.Azure.Devices.ServiceClient serviceClient;
        private static string connectionString = "HostName=skelton-test-hub.azure-devices.net;DeviceId=MyCDevice;SharedAccessKey=KPOS6risRzDE9E/p5aLaRev1vE5ENfqBUYmOluma++Q=";
        private static string deviceId = "MyCDevice";
        public static string scanResult;

        public ICommand InsertCommand { get; }

        public ICommand FridgeLinkCommand { get; }

        public ICommand DeleteCommand { get; }
        
        public ICommand RefrigiratorContentCommand { get; }
        
        public ICommand PurchaseListCommand { get; }

        public ICommand AccountSettingsCommand { get; }

        public ICommand SignOutCommand { get; }

        public ICommand LeaveFridgeCommand { get; }

        public string FridgeName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            Application.Current.Properties["IsLoggedIn"] = Boolean.TrueString;
            FridgeName = Application.Current.Properties["fridgename"].ToString();
            InsertCommand = new Command(insert);
            DeleteCommand = new Command(delete);
            FridgeLinkCommand = new Command(fridgeLink);
            RefrigiratorContentCommand = new Command(view_content);
            PurchaseListCommand = new Command(view_purchase_list);
            AccountSettingsCommand = new Command(AccountSettings);
            SignOutCommand = new Command(SignOut);
            LeaveFridgeCommand = new Command(LeaveFridge);
            
            Device.BeginInvokeOnMainThread(async () => {
                bool empty = await changesLogIsEmpty();
                if (! empty)
                {
                    string result = await writeChangesInFridge();
                    await Application.Current.MainPage.DisplayAlert("Welcome to " + Application.Current.Properties["fridgename"].ToString(), "The last changes that were made in the fridge are:\n" + result, "Ok");
                }    
            });  
            
            connection = new HubConnectionBuilder()
                .WithUrl("https://counterfunctions20200502095136.azurewebsites.net/api/").Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();  
            };

            try
            {
                connection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        async Task<bool> changesLogIsEmpty()
        {
            List<string> table = await tableFunctions.get_column(Application.Current.Properties["fridgename"].ToString() + "LastActivity", "content");
            return ((table.Count == 0) || ((table.Count==1) && (table[0].Length==0)));
        }

        async Task<string> writeChangesInFridge()
        {
            string result = "";
            List<string> table = await tableFunctions.get_column(Application.Current.Properties["fridgename"].ToString() + "LastActivity", "content");
            int index = 0;
            foreach(string element in table)
            {
                index++;
                result += element;
                if (index < table.Count)
                {
                    result += "\n";
                }
            }
            return result;
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LeaveFridge()
        {
            bool answer = await Application.Current.MainPage.DisplayAlert("", "Are you sure you want to leave fridge sharing?", "Yes", "No");
            if (answer)
            {
                var AccountVM = new AccountSettingsPageViewModel();
                var AccountSettingsXaml = new AccountSettingsPage();
                AccountSettingsXaml.BindingContext = AccountVM;
                Application.Current.MainPage.Navigation.InsertPageBefore(AccountSettingsXaml, Application.Current.MainPage.Navigation.NavigationStack[0]);
                await Application.Current.MainPage.Navigation.PopToRootAsync();

                var SignUpVM = new SignUpPageViewModel();
                var SignUpXaml = new SignUpPage();
                SignUpXaml.BindingContext = SignUpVM;
                Application.Current.MainPage.Navigation.InsertPageBefore(SignUpXaml, Application.Current.MainPage.Navigation.NavigationStack[0]);
            }
        }

        private async void fridgeLink()
        {
            string prefix = "Hi, please join my fridge sharing\n";
            string name = "Fridge Name:" + Application.Current.Properties["fridgename"].ToString();
            string ID = await tableFunctions.get_value("FridgesTable", "Id", Application.Current.Properties["fridgename"].ToString());
            string id = "Fridge ID:" +  ID;
            string to_clibpoard = prefix + name + "\n" + id;
            await Clipboard.SetTextAsync(to_clibpoard);
            await Application.Current.MainPage.DisplayAlert("", "Fridge properties copied", "OK");
        }

        async Task<bool> qr_code_exists(string sResult)
        {
            String FName = Application.Current.Properties["fridgename"].ToString();
            List<String> column = await tableFunctions.get_column(FName + "Content", "qr");
            return column.Contains(sResult);
        }

        private async void insert()
        {
            var scan = new ZXingScannerPage();
            await Application.Current.MainPage.Navigation.PushAsync(scan);
            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    scanResult = result.Text;
                    if (await qr_code_exists(scanResult))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Qr Code is already assigned to another product", "OK");
                    }
                    else
                    {
                        var InsertVM = new InsertPageViewModel(scanResult);
                        var InsertPageXaml = new InsertPage();
                        InsertPageXaml.BindingContext = InsertVM;
                        await Application.Current.MainPage.Navigation.PopAsync();
                        await Application.Current.MainPage.Navigation.PushAsync(InsertPageXaml);
                        InsertVM.OnInsertResult += (insertResult) =>
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await Application.Current.MainPage.Navigation.PopAsync(true);
                            });
                        };
                    }
                });

            };
        }

        public async Task<bool> free_user_from_fridge(String fridgename, String username)
        {
            await tableFunctions.delete_from_table(fridgename+"Users", username, "RowKey");
            return true;
        }

        private async void AccountSettings()
        {
            bool b = await free_user_from_fridge(Application.Current.Properties["fridgename"].ToString(), Application.Current.Properties["username"].ToString());
            Application.Current.Properties["fridgename"] = null;
            Application.Current.Properties["IsLoggedIn"] = Boolean.FalseString;

            var SignUpVM = new SignUpPageViewModel();
            var SignUpXaml = new SignUpPage();
            SignUpXaml.BindingContext = SignUpVM;
            Application.Current.MainPage.Navigation.InsertPageBefore(SignUpXaml, Application.Current.MainPage.Navigation.NavigationStack[0]);
            
            var AccountVM = new AccountSettingsPageViewModel();
            var AccountSettingsXaml = new AccountSettingsPage();
            AccountSettingsXaml.BindingContext = AccountVM;
            Application.Current.MainPage.Navigation.InsertPageBefore(AccountSettingsXaml, Application.Current.MainPage.Navigation.NavigationStack[1]);

            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async void SignOut()
        {
            bool wasLoggedIn = Application.Current.Properties.ContainsKey("WasLoggedIn") ? Convert.ToBoolean(Application.Current.Properties["WasLoggedIn"]) : false;
            Application.Current.Properties["IsLoggedIn"] = Boolean.FalseString;
            bool b = await free_user_from_fridge(Application.Current.Properties["fridgename"].ToString(), Application.Current.Properties["username"].ToString());
            Application.Current.Properties["fridgename"] = null;
            Application.Current.Properties["username"] = null;
            var SignUpVM = new SignUpPageViewModel();
            var SignUpXaml = new SignUpPage();
            SignUpXaml.BindingContext = SignUpVM;
            Application.Current.MainPage.Navigation.InsertPageBefore(SignUpXaml, Application.Current.MainPage.Navigation.NavigationStack[0]);
            
            await Application.Current.MainPage.Navigation.PopToRootAsync(true);
            Application.Current.Properties["fridgename"] = null;
            Application.Current.Properties["username"] = null;
        }

        private async void delete()
        {
            var scan = new ZXingScannerPage();
            String fName = Application.Current.Properties["fridgename"].ToString();
            
            await Application.Current.MainPage.Navigation.PushAsync(scan);
            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.Navigation.PopAsync(true);
                    scanResult = result.Text;
                    if (!(await qr_code_exists(scanResult)))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Qr Code does not exist", "OK");
                    }
                    else
                    {
                        string productAndCount = await tableFunctions.delete_from_table(fName+"Content",scanResult,"qr");
                        int indexOfDollar = productAndCount.IndexOf('$');
                        string product = productAndCount.Substring(0, indexOfDollar);
                        string pcount = "";
                        if(productAndCount.Length > indexOfDollar+1)
                        {
                            pcount = productAndCount.Substring(indexOfDollar + 1);
                        }
                        await tableFunctions.add_to_last_activity_list(fName + "LastActivity", Application.Current.Properties["username"].ToString(), "removed "+pcount, product);
                    }
                });
            };
        }

        private async void view_content()
        {
            var ContentVM = new ViewContentViewModel();
            var ViewContentXaml = new ViewContent();
            ViewContentXaml.BindingContext = ContentVM;
            await Application.Current.MainPage.Navigation.PushAsync(ViewContentXaml);
        }
        private async void view_purchase_list()
        {
            var ShoppingVM = new ShoppingListViewModel();
            var ShoppingXaml = new ShoppingListPage();
            ShoppingXaml.BindingContext = ShoppingVM;
            await Application.Current.MainPage.Navigation.PushAsync(ShoppingXaml);
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}