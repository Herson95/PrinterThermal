
using Android.Bluetooth;
using Android.Content;
using Android.Widget;
using PrinterThermal.DependencyServices;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PrinterThermal.Droid.DependencyServices
{
    public class BluetoothDeviceReceiver : BroadcastReceiver
    {
        public static BluetoothAdapter Adapter => BluetoothAdapter.DefaultAdapter;

        public async override void OnReceive(Context context, Intent intent)
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
                        AndroidBlueToothService.GetDeviceLis($"{device.Name} {device.Address}");
                        Toast.MakeText(MainActivity.Context, $"{device.Name} {device.Address}", ToastLength.Long).Show();
                    }

                    break;
                case BluetoothAdapter.ActionDiscoveryStarted:
                    Toast.MakeText(MainActivity.Context, "Scanning Started...",ToastLength.Long).Show();
                    break;
                case BluetoothAdapter.ActionDiscoveryFinished:
                    //Toast.MakeText(MainActivity.Context, "Discovery Finished.", ToastLength.Long).Show();
                    break;
                default:
                    break;
            }
        }
    }
}