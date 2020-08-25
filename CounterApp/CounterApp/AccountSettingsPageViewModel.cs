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
    public class AccountSettingsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand NewFridgeCommand { get; }
        public ICommand JoinFridgeCommand { get; }
        public ICommand MyFridgesCommand { get; }

        public AccountSettingsPageViewModel()
        {
            NewFridgeCommand = new Command(NewFridgeCmd);
            JoinFridgeCommand = new Command(JoinFridgeCmd);
        }
        private async void NewFridgeCmd()
        {
            var NewFridgeVM = new NewFridgePageViewModel();
            var NewFridgeXaml = new NewFridgePage();
            NewFridgeXaml.BindingContext = NewFridgeVM;
            await Application.Current.MainPage.Navigation.PushAsync(NewFridgeXaml);
        }

        private async void JoinFridgeCmd()
        {
            var JoinFridgeVM = new JoinFridgePageViewModel();
            var JoinFridgeXaml = new JoinFridgePage();
            JoinFridgeXaml.BindingContext = JoinFridgeVM;
            await Application.Current.MainPage.Navigation.PushAsync(JoinFridgeXaml);
        }
    }
}
