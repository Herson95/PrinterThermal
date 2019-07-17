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
    using PrinterThermal.DependencyServices;

    public class AndroidBlueToothService : IBlueToothService
    {
        public static List<string> Devices { get; set; } = new List<string>();
        private bool _isReceiveredRegistered;
        private BluetoothDeviceReceiver _receiver;


        public AndroidBlueToothService()
        {
            _receiver = new BluetoothDeviceReceiver();

            //RegisterBluetoothReceiver();
            //StartScanning();
        }

        public IList<string> GetPairedDevice()
        {
            var devices = new List<string>();
            // Register for broadcasts when a device is discovered
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                if (bluetoothAdapter != null)
                {
                    devices = bluetoothAdapter?.BondedDevices.Select(i => i.Name + " " + i.Address).ToList();
                }

            }
            return devices;

        }

        public static IList<string> GetDeviceLis(string device = "")
        {

            var devices = new List<string>();
            
            if (device != "")
            {
                devices.Add(device);
            }
            Devices = new List<string>(devices);
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

            await Task.Delay(8000);

            if (!BluetoothDeviceReceiver.Adapter.IsDiscovering)
            {
                return true;
            }
            else
            {
                await ScanDeviceNoPaired();
            }

            return false;
        }

        public IList<string> GetNoPairedDevice()
        {
            Toast.MakeText(MainActivity.Context, "Scanning Finished...", ToastLength.Long).Show();
            return Devices;
        }
    }
}