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
    public class Fridge
    {
        public string Name { get; set; }
        public string ID { get; set; }

        

        public Fridge(string Name, string ID)
        {
            this.Name = Name;
            this.ID = ID;
            

        }
    }
}
