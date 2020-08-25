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
    public class InsertPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        String the_product;
        String product_count;
        public delegate void InsertDelegate(int result);
        public event InsertDelegate OnInsertResult;
        
        public String TheProduct
        {
            get => the_product;
            set
            {
                the_product = value;
                var args = new PropertyChangedEventArgs(nameof(TheProduct));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public String ProductCount
        {
            get => product_count;
            set
            {
                product_count = value;
                var args = new PropertyChangedEventArgs(nameof(ProductCount));
                PropertyChanged?.Invoke(this, args);
            }
        }
        public ICommand SaveCommand { get; }
       
        public ObservableCollection<String> AllProducts { get; set; }

        String qrString;

        public InsertPageViewModel()
        {
            qrString = "";
        }

            public InsertPageViewModel(String qr)
        {
            qrString = qr;
            AllProducts = new ObservableCollection<string>();

            SaveCommand =  new Command(saveCmd);
        }

        private async void saveCmd()
        {
            if ((JoinFridgePageViewModel.invalid_input(ProductCount)) || (JoinFridgePageViewModel.invalid_input(TheProduct)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Name or count fields are empty", "OK");
            }
            else if ((!int.TryParse(ProductCount, out int c)) || (c <= 0))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Count must be a positive number", "OK");
            }
            else
            {
                AllProducts.Add(TheProduct);
                
                insertToCloudTable(TheProduct,qrString , ProductCount, Convert.ToString(Application.Current.Properties["fridgename"]));
                TheProduct = string.Empty;
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            ProductCount = null;
            TheProduct = null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        async Task<bool> product_exists(String p)
        {
            String FName = Application.Current.Properties["fridgename"].ToString();
            List<String> column = await tableFunctions.get_column(FName + "Content", "RowKey");
            return column.Contains(p);
        }

        public async void insertToCloudTable(String product, String QR, String Pcount,String Fname)
        {
            if ((JoinFridgePageViewModel.invalid_input(Pcount)) || (JoinFridgePageViewModel.invalid_input(product)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Name or count fields are empty", "OK");
            }
            else if ((!int.TryParse(Pcount, out int c)) || (c <= 0))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Count must be a positive number", "OK");
            }
            else if (await product_exists(product))
            {
                await Application.Current.MainPage.DisplayAlert("", "The product already has a Qr code assigned to it", "OK");
            }
            else
            {
                List<String> arg = new List<String>();
                arg.Add(product);
                arg.Add(Pcount);
                arg.Add(QR);
                await tableFunctions.add_to_table("contentWithQR", Fname + "Content",arg);
                await tableFunctions.add_to_last_activity_list(Fname + "LastActivity", Application.Current.Properties["username"].ToString(),"added "+Pcount,product);

                List<String> productsInShopping = await tableFunctions.get_column(Fname + "Shopping", "RowKey");
                List<String> productsInShoppingLower = productsInShopping.ConvertAll(d => d.ToLower());
                String productLower = product.ToLower();
                if (productsInShoppingLower.Contains(productLower))
                {
                    int index2 = productsInShoppingLower.IndexOf(productLower);
                    String count = await tableFunctions.get_value(Fname + "Shopping", "count", productsInShopping[index2]);
                    Int32 num = Int32.Parse(count);
                    num-= Int32.Parse(Pcount);
                    if (num > 0)
                    {
                        bool b = await tableFunctions.set_value(Fname + "Shopping", "count", productsInShopping[index2], num.ToString());
                    }
                    else
                    {
                        await tableFunctions.delete_from_table(Fname + "Shopping", productsInShopping[index2], "RowKey");
                    }
                }
            }
        }

    }
}