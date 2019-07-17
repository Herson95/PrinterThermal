
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Hardware.Usb;
using Android.Widget;
using System;

namespace PrinterThermal.Droid.DependencyServices
{
    public class CustomBroadcastReceiver : BroadcastReceiver
    {

        public event EventHandler DeviceDiscoveryStarted;

        public event EventHandler DeviceDiscoverEnded;


        public CustomBroadcastReceiver()
        {

        }

        public override void OnReceive(Context context, Intent intent)
        {

            String action = intent.Action;

            if (BluetoothAdapter.ActionDiscoveryStarted.Equals(action))
            {
                DoDeviceDiscoveryStarted();
            }
            else if (BluetoothAdapter.ActionDiscoveryFinished.Equals(action))
            {
                DoDeviceDiscoverEnded();
            }
            else if (BluetoothDevice.ActionFound.Equals(action))
            {
                System.Diagnostics.Debug.WriteLine("Found a device");
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            }
        }

        public void DoDeviceDiscoveryStarted()
        {
            if (DeviceDiscoveryStarted != null)
            {
                DeviceDiscoveryStarted(this, new EventArgs());
            }
        }

        public void DoDeviceDiscoverEnded()
        {
            if (DeviceDiscoverEnded != null)
            {
                DeviceDiscoverEnded(this, new EventArgs());
            }
        }

    }
}