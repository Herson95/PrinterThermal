using PrinterThermal.Droid.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidBlueToothService))]
namespace PrinterThermal.Droid.DependencyServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Bluetooth;
    using Android.Content;
    using Android.Widget;
    using Java.Util;
    using PrinterThermal.DependencyServices;

    public class AndroidBlueToothService : IBlueToothService
    {
        public static UUID UUID { get; set; } = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
        public static List<string> Devices { get; set; } = new List<string>();
        public static List<BluetoothDevice> DevicesBluetooth { get; set; }
        private bool _isReceiveredRegistered;
        private BluetoothDeviceReceiver _receiver = new BluetoothDeviceReceiver();

        public async Task<IList<string>> GetPairedDevice()
        {
            var devices = new List<string>();
         
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                if (!bluetoothAdapter.IsEnabled)
                {
                    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                    MainActivity.Context.StartActivity(enableBtIntent);
                    await Task.Delay(5000);
                    if (!bluetoothAdapter.IsEnabled)
                    {
                        return devices;
                    }
                }
                if (bluetoothAdapter != null)
                {
                    devices = bluetoothAdapter?.BondedDevices.Select(i => i.Name + " " + i.Address).ToList();
                }

            }
            return devices;

        }


        private static void StartScanning()
        {
            if (!BluetoothDeviceReceiver.Adapter.IsDiscovering) BluetoothDeviceReceiver.Adapter.StartDiscovery();
        }

        private static void CancelScanning()
        {
            if (BluetoothDeviceReceiver.Adapter.IsDiscovering) BluetoothDeviceReceiver.Adapter.CancelDiscovery();
        }

        private void RegisterBluetoothReceiver()
        {
            if (_isReceiveredRegistered) return;

            MainActivity.Context.RegisterReceiver(_receiver, new IntentFilter(BluetoothDevice.ActionFound));
            MainActivity.Context.RegisterReceiver(_receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted));
            MainActivity.Context.RegisterReceiver(_receiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            _isReceiveredRegistered = true;
        }


        private void UnregisterBluetoothReceiver()
        {
            if (!_isReceiveredRegistered) return;

            MainActivity.Context.UnregisterReceiver(_receiver);
            _isReceiveredRegistered = false;
        }


        public async Task<bool> ScanDeviceNoPaired()
        {
           
            RegisterBluetoothReceiver();
            StartScanning();
            do
            {
                await Task.Delay(1000);
            } while (BluetoothDeviceReceiver.Adapter.IsDiscovering);

            return true;
           
        }

        public IList<string> GetNoPairedDevice()
        {
            return Devices;
        }

        public async Task<bool> PairedDevice(string deviceAddress)
        {
            bool response = true;
            var address = deviceAddress.Split(" ").LastOrDefault();
            var device = (from bd in DevicesBluetooth
                          where bd?.Address == address
                          select bd).FirstOrDefault();
            var BluetoothSocket = device?.CreateRfcommSocketToServiceRecord(UUID);
            try
            {
                await BluetoothSocket?.ConnectAsync();
                BluetoothSocket.Close();
                BluetoothSocket.Dispose();
            }
            catch
            {
                response = false;
            }
            return response;
        }
    }
}