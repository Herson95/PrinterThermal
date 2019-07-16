namespace PrinterThermal.ViewModels
{
    using DependencyServices;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
                if (SelectedConnection == "USB")
                {
                    BindUsbDeviceList();
                    type = "3";
                }
                else
                {
                    BindDeviceList();
                    type = "2";
                }
            }
        }

        private string _printMessage;
        public string PrintMessage
        {
            get
            {
                return _printMessage;
            }
            set
            {
                SetProperty(ref _printMessage, value);
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

        public ICommand PrintCommand => new Command(async () =>
        {
            try
            {
                var printer = DependencyService.Get<IPrinterService>();
                if (string.IsNullOrEmpty(SelectedDevice))
                {
                    return;
                }
                if (type=="3")
                {
                    var c = SelectedDevice.Split(' ');
                    var vendorID = int.Parse(c[1]);
                    var productID = int.Parse(c[2]);
                    uSBService.ConnectAndSend(productID,vendorID);
                    return;
                }
                await printer.Test("", 9100, SelectedDevice, 0, type);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "Ok");
            }

        });

        public PrinterPageViewModel()
        {
            _blueToothService = DependencyService.Get<IBlueToothService>();
            uSBService = DependencyService.Get<IUSBService>();
        }

        void BindDeviceList()
        {
            var list = _blueToothService.GetDeviceList();
            DeviceList.Clear();
            foreach (var item in list)
                DeviceList.Add(item);
        }

        void BindUsbDeviceList()
        {
            var list = uSBService.GetDeviceList();
            DeviceList.Clear();
            foreach (var item in list)
                DeviceList.Add(item);
        }
    }
}
