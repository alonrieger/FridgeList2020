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
using Newtonsoft.Json;
using System.Data;
using System.Collections;
using Microsoft.Azure.Devices.Common;

namespace CounterApp
{
    public class ShoppingListViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public String Name;
        public String Count;

        public String name
        {
            get => Name;
            set
            {
                Name = value;
                var args = new PropertyChangedEventArgs(nameof(name));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public String count
        {
            get => Count;
            set
            {
                Count = value;
                var args = new PropertyChangedEventArgs(nameof(count));
                PropertyChanged?.Invoke(this, args);
            }
        }

        public DataTable table { get; set; }

        public ObservableCollection<(string, string)> AllPrdouctsAndCounts { get; set; }

        public List<ProductInFridge> arr1 { get; set; }

        public ObservableCollection<string> ProductsAndCountsAsString { get; set; }

        public ObservableCollection<ProductInFridge> productsInList { get; set; }

        public ObservableCollection<Dictionary<string, string>> DictOfAllProducts { get; set; }

        public ICommand increment { get; }

        public ICommand decrement { get; }

        public ICommand insert { get; }

        public int _counter2;

        public int Counter2
        {
            get => _counter2;
            set => SetProperty(ref _counter2, value);
        }

        public string _counter3;

        public string Counter3
        {
            get => _counter3;
            set => SetProperty(ref _counter3, value);
        }

        public ShoppingListViewModel()
        {
            increment = new Command<String>(incrementCmd);
            decrement = new Command<String>(decrementCmd);
            insert = new Command(insertCmd);

            AllPrdouctsAndCounts = new ObservableCollection<(string, string)>();
            ProductsAndCountsAsString = new ObservableCollection<string>();
            DictOfAllProducts = new ObservableCollection<Dictionary<string, string>>();
            productsInList = new ObservableCollection<ProductInFridge>();
            table = new DataTable();
            arr1 = new List<ProductInFridge>();
            view_content(Convert.ToString(Application.Current.Properties["fridgename"]));
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

        private async void incrementCmd(String PName)
        {
            String FName = Convert.ToString(Application.Current.Properties["fridgename"]);
            for (int index = 0; index < productsInList.Count; index++)
            {
                if (string.Equals(PName, productsInList[index].Name))
                {
                    Int32 num = Int32.Parse(productsInList[index].Count);
                    num++;
                    productsInList.Remove(productsInList[index]);
                    productsInList.Insert(index, new ProductInFridge(PName, num.ToString()));
                    bool b = await tableFunctions.set_value(FName + "Shopping",  "count", PName, num.ToString());
                }
            }
        }
        private async void decrementCmd(String PName)
        {
            String FName = Convert.ToString(Application.Current.Properties["fridgename"]);
            for (int index = 0; index < productsInList.Count; index++)
            {
                if (string.Equals(PName, productsInList[index].Name))
                {
                    Int32 num = Int32.Parse(productsInList[index].Count);
                    num--;
                    if (num > 0)
                    {
                        productsInList.Remove(productsInList[index]);
                        productsInList.Insert(index, new ProductInFridge(PName, num.ToString()));
                        bool b = await tableFunctions.set_value(FName + "Shopping",  "count", PName, num.ToString());
                    }
                    else
                    {
                        productsInList.Remove(productsInList[index]);
                        await tableFunctions.delete_from_table(FName + "Shopping", PName, "RowKey");
                    }
                }
            }
        }
        private async void insertCmd()
        {
            String FName = Convert.ToString(Application.Current.Properties["fridgename"]);
            if ((JoinFridgePageViewModel.invalid_input(count)) || (JoinFridgePageViewModel.invalid_input(name)))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Name or count fields are empty", "OK");
            }
            else if ((!int.TryParse(count,out int c)) || (c<=0))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Count must be a positive number", "OK");
            }

            else
            {
                bool changed = false;
                for (int index = 0; index < productsInList.Count; index++)
                {
                    if (string.Equals(name.ToLower(), productsInList[index].Name.ToLower()))
                    {
                        Int32 num = Int32.Parse(productsInList[index].Count);
                        num += c;
                        string N = productsInList[index].Name;
                        productsInList.Remove(productsInList[index]);
                        productsInList.Insert(index, new ProductInFridge(N, num.ToString()));
                        changed = true;
                        bool b = await tableFunctions.set_value(FName + "Shopping",  "count", productsInList[index].Name, num.ToString());
                    }
                }
                if (!changed)
                {
                    productsInList.Add(new ProductInFridge(name, count));
                    List<String> arg = new List<String>();
                    arg.Add(name);
                    arg.Add(c.ToString());
                    await tableFunctions.add_to_table("shopping", FName + "Shopping", arg);
                }
            }
            name = null;
            count = null;
        }
        public async void view_content(String FName)
        {
            List<String> products = await tableFunctions.get_column(FName + "Shopping", "RowKey");
            List<String> counts = await tableFunctions.get_column(FName + "Shopping", "count");
            Dictionary<String, String> values = new Dictionary<String, String>();

            for (int index = 0; index < products.Count; index++)
            {
                values.Add(products[index], counts[index]);
            }

            table.Columns.Add("Col1", typeof(string));
            table.Columns.Add("Col2", typeof(string));
            DictOfAllProducts.Add(values);
            string to_add = "";
            if (values != null)
            {
                foreach (KeyValuePair<string, string> item in values)
                {
                    ProductInFridge current_product = new ProductInFridge(item.Key, item.Value);
                    productsInList.Add(current_product);
                    to_add += item.Key;
                    to_add += " : ";
                    to_add += item.Value;

                    ProductsAndCountsAsString.Add(to_add);
                    arr1.Add(current_product);

                    AllPrdouctsAndCounts.Add((item.Key, item.Value));
                    table.Rows.Add(item.Key, item.Value);
                    Counter3 = item.Key;
                    Counter2 += 1;
                    Console.WriteLine("Key: {0}, Value: {1}", item.Key, item.Value);
                    to_add = "";
                }
            }

        }
    }
}
