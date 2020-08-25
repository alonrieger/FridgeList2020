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
using Xamarin.Forms.Internals;

namespace CounterApp
{
    public class ViewContentViewModel : INotifyPropertyChanged
    {
        public DataTable table { get; set; }

        public ICommand increment { get; }

        public ICommand decrement { get; }

        public ObservableCollection<(string, string)> AllPrdouctsAndCounts { get; set; }

        public ObservableCollection<Dictionary<string, string>> DictOfAllProducts { get; set; }

        public ObservableCollection<string> ProductsAndCountsAsString { get; set; }

        public List<ProductInFridge> arr1 { get; set; } 

        public ObservableCollection<ProductInFridge> productsInFridge { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<string, string> dict;

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

        public ViewContentViewModel()
        {
            increment = new Command<String>(incrementCmd);
            decrement = new Command<String>(decrementCmd);

            AllPrdouctsAndCounts = new ObservableCollection<(string, string)>();
            ProductsAndCountsAsString = new ObservableCollection<string>();
            DictOfAllProducts = new ObservableCollection<Dictionary<string, string>>();
            productsInFridge = new ObservableCollection<ProductInFridge>();
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

        public async void incrementCmd(String PName)
        {
            String FName = Convert.ToString(Application.Current.Properties["fridgename"]);
            for(int index=0; index < productsInFridge.Count; index++)
            {
                if(string.Equals(PName, productsInFridge[index].Name))
                {
                    Int32 num = Int32.Parse(productsInFridge[index].Count);
                    num++;
                    productsInFridge.Remove(productsInFridge[index]);
                    productsInFridge.Insert(index, new ProductInFridge(PName, num.ToString()));
                    bool b = await tableFunctions.set_value(FName + "Content",  "count", PName, num.ToString());
                    List<String> productsInShopping = await tableFunctions.get_column(FName + "Shopping", "RowKey");

                    String PNameLower = PName.ToLower();
                    List<String> productsInShoppingLower = productsInShopping.ConvertAll(d => d.ToLower());
                    if (productsInShoppingLower.Contains(PNameLower))
                    {
                        int index2 = productsInShoppingLower.IndexOf(PNameLower);
                        String count = await tableFunctions.get_value(FName + "Shopping", "count", productsInShopping[index2]);
                        Int32 c = Int32.Parse(count);
                        c--;
                        if (c > 0)
                        {
                            bool bo = await tableFunctions.set_value(FName + "Shopping",  "count", productsInShopping[index2], c.ToString());
                        }
                        else
                        {
                            await tableFunctions.delete_from_table(FName + "Shopping", productsInShopping[index2], "RowKey");
                        }
                        await tableFunctions.add_to_last_activity_list(FName + "LastActivity", Application.Current.Properties["username"].ToString(), "added " + "1", PName);
                    }
                }
            }
        }

        public async void decrementCmd(String PName)
        {
            String FName = Convert.ToString(Application.Current.Properties["fridgename"]);
            for (int index = 0; index < productsInFridge.Count; index++)
            {
                if (string.Equals(PName, productsInFridge[index].Name))
                {
                    Int32 num = Int32.Parse(productsInFridge[index].Count);
                    num--;
                    if (num > 0)
                    {
                        productsInFridge.Remove(productsInFridge[index]);
                        productsInFridge.Insert(index ,new ProductInFridge(PName,num.ToString()));
                        bool b = await tableFunctions.set_value(FName + "Content",  "count", PName, num.ToString());
                    }
                    else
                    {
                        productsInFridge.Remove(productsInFridge[index]);
                        await tableFunctions.delete_from_table(FName + "Content", PName, "RowKey");
                    }
                    await tableFunctions.add_to_last_activity_list(FName + "LastActivity", Application.Current.Properties["username"].ToString(), "removed " + "1", PName);
                }
            }
        }


        public async void view_content(String FName)
        {
            List<String> products = await tableFunctions.get_column(FName + "Content", "RowKey");
            List<String> counts = await tableFunctions.get_column(FName + "Content", "count");
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
                    productsInFridge.Add(current_product);
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

