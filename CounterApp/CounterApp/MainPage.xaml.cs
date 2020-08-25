using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Newtonsoft.Json;



using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
//using Microsoft.Azure.Devices;
using System.Text;

namespace CounterApp
{

    public partial class MainPage : ContentPage
    {
        HubConnection connection;

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
