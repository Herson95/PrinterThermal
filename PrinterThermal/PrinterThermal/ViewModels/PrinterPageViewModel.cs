namespace PrinterThermal.ViewModels
{
    using DependencyServices;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Xamarin.Forms;

    public class PrinterPageViewModel : BaseViewModel
    {
        private readonly IBlueToothService _blueToothService;
        private readonly IUSBService uSBService;
        private string type = "1";

        private IList<string> _deviceList;
        public IList<string> DeviceList
        {
            get
            {
                if (_deviceList == null)
                    _deviceList = new ObservableCollection<string>();
                return _deviceList;
            }
            set
            {
                SetProperty(ref _deviceList, value);
            }
        }

        private IList<string> _deviceListNoPaired;
        public IList<string> DeviceListNoPaired
        {
            get
            {
                if (_deviceListNoPaired == null)
                    _deviceListNoPaired = new ObservableCollection<string>();
                return _deviceListNoPaired;
            }
            set
            {
                SetProperty(ref _deviceListNoPaired, value);
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                SetProperty(ref isEnabled, value);
            }
        }

        private bool isEnabledButtons;
        public bool IsEnabledButtons
        {
            get
            {
                return isEnabledButtons;
            }
            set
            {
                SetProperty(ref isEnabledButtons, value);
            }
        }


        public ObservableCollection<string> ConnectionList => new ObservableCollection<string>
        {
            "Bluetooth",
            "USB",
        };

        private string _selectedList;
        public string SelectedConnection
        {
            get
            {
                return _selectedList;
            }
            set
            {
                SetProperty(ref _selectedList, value);
                if (string.IsNullOrEmpty(SelectedConnection))
                {
                    return;
                }
                
            }
        }

        private string _selectedDeviceNoPaired;
        public string SelectedDeviceNoPaired
        {
            get
            {
                return _selectedDeviceNoPaired;
            }
            set
            {
                SetProperty(ref _selectedDeviceNoPaired, value);
            }
        }

        private string _selectedDevice;
        public string SelectedDevice
        {
            get
            {
                return _selectedDevice;
            }
            set
            {
                SetProperty(ref _selectedDevice, value);
            }
        }

        public ICommand PrintCommand { get; set; }

        public PrinterPageViewModel()
        {
            IsEnabledButtons = true;
            _blueToothService = DependencyService.Get<IBlueToothService>();
            uSBService = DependencyService.Get<IUSBService>();
            PrintCommand = new Command<string>(Operation);
        }

        private void Operation(string obj)
        {
            IsEnabledButtons = false;
            if (obj.Equals("Scan"))
            {
                ScanDevice();
            }
            else if (obj.Equals("Paired"))
            {
                Paired();
            }
            else
            {
                Print();
            }
           
        }

        private async Task DevicesBluetoothPaired()
        {
            try
            {
                IsBusy = true;
                IsEnabled = true;
                var list = await _blueToothService.GetPairedDevice();
                DeviceList = new List<string>(list);
                IsBusy = false;
                IsEnabledButtons = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", ex.Message,"OK");
                IsBusy = false;
                IsEnabledButtons = true;
            }
        }

        private async Task DevicesBluetoothNoPaired()
        {
            try
            {
                IsBusy = true;
                IsEnabled = true;
                IsEnabledButtons = false;
                await _blueToothService.ScanDeviceNoPaired();
                var list2 = _blueToothService.GetNoPairedDevice();
                DeviceListNoPaired = new List<string>(list2);
                IsBusy = false;
                IsEnabledButtons = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "OK");
                IsBusy = false;
                IsEnabledButtons = true;
            }
        }

        private void DevicesUSB()
        {
            IsBusy = true;
            IsEnabled = false;
            uSBService.CreateConnection();
            var list = uSBService.GetDeviceList();
            DeviceList = new List<string>(list);
            IsBusy = false;
            IsEnabledButtons = true;
        }

        private async void ScanDevice()
        {
            if (string.IsNullOrEmpty(SelectedConnection))
            {
                await Application.Current.MainPage.DisplayAlert("Alert", "Please select connection type.", "Ok");
                IsEnabledButtons = true;
                return;
            }
            if (SelectedConnection == "USB")
            {
                DevicesUSB();
                type = "3";
            }
            else
            {
                await DevicesBluetoothPaired();
                await DevicesBluetoothNoPaired(); 
                type = "2";
            }
        }

        private async void Print()
        {
            try
            {
                var printer = DependencyService.Get<IPrinterService>();
                if (string.IsNullOrEmpty(SelectedDevice))
                {
                    IsEnabledButtons = true;
                    return;
                }
                if (type == "3")
                {
                    var c = SelectedDevice.Split(' ');
                    var vendorID = int.Parse(c[0]);
                    var productID = int.Parse(c[1]);
                    uSBService.ConnectAndSend(productID, vendorID);
                    IsEnabledButtons = true;
                    return;
                }
                await printer.Test("", 9100, SelectedDevice, 0, type);
                IsEnabledButtons = true;
            }
            catch (Exception ex)
            {
                IsEnabledButtons = true;
                await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "Ok");
            }
        }

        private async void Paired()
        {
            if (string.IsNullOrEmpty(SelectedDeviceNoPaired))
            {
                await Application.Current.MainPage.DisplayAlert("Alert", "Select device no paired", "Ok");
                IsEnabledButtons = true;
                return;
            }
            IsBusy = true;
            var response= await _blueToothService.PairedDevice(SelectedDeviceNoPaired);
            if (response)
            {
               await DevicesBluetoothNoPaired();
               await DevicesBluetoothPaired();
            }
            IsBusy = false;
            IsEnabledButtons = true;
        }
    }
}
