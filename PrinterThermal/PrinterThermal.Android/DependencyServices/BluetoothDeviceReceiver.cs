
using Android.Bluetooth;
using Android.Content;
using Android.Widget;
using Java.Util;
using PrinterThermal.DependencyServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PrinterThermal.Droid.DependencyServices
{
    public class BluetoothDeviceReceiver : BroadcastReceiver
    {
        public static BluetoothAdapter Adapter => BluetoothAdapter.DefaultAdapter;
        public static List<string> Devices { get; set; } = new List<string>();
        public static List<BluetoothDevice> DevicesBluetooth { get; set; } = new List<BluetoothDevice>();

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            
            // Found a device
            switch (action)
            {
                case BluetoothDevice.ActionFound:
                    // Get the device
                    var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    // Only update the adapter with items which are not bonded
                    if (device.BondState != Bond.Bonded)
                    {
                        Devices.Add($"{device.Name} {device.Address}");
                        DevicesBluetooth.Add(device);
                        Toast.MakeText(MainActivity.Context, $"{device.Name} {device.Address}", ToastLength.Long).Show();
                    }

                    break;
                case BluetoothAdapter.ActionDiscoveryStarted:
                    Toast.MakeText(MainActivity.Context, "Scanning Started...",ToastLength.Long).Show();
                    break;
                case BluetoothAdapter.ActionDiscoveryFinished:
                    Toast.MakeText(MainActivity.Context, "Scanning Finished.", ToastLength.Long).Show();
                    AndroidBlueToothService.Devices = new List<string>(Devices);
                    AndroidBlueToothService.DevicesBluetooth = new List<BluetoothDevice>(DevicesBluetooth);

                    DevicesBluetooth = new List<BluetoothDevice>();
                    Devices = new List<string>();

                    break;
                default:
                    break;
            }
        }
    }
}