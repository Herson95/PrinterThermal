namespace PrinterThermal.Droid.DependencyServices
{
    using Android.Bluetooth;
    using Android.Content;
    using Android.Widget;
    using System.Collections.Generic;
    using System.Linq;
    using Android.Hardware.Usb;
    using Android;
    using Android.Util;
    using Android.App;

    public class UsbReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            //if (MainActivity.ACTION_USB_PERMISSION.Equals(action))
            //{
            //    UsbDevice device = (UsbDevice)intent.GetParcelableExtra(UsbManager.ExtraDevice);

            //    if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
            //    {
            //        if (device != null)
            //        {
            //            //call method to set up device communication
            //            Toast.MakeText(MainActivity.Context, $"Connected", ToastLength.Long).Show();
            //        }
            //    }
            //    else
            //    {
            //        Toast.MakeText(MainActivity.Context, "permission denied for device " + device.DeviceId, ToastLength.Long).Show();
            //        Log.Debug("Tag", $"Permission denied");
            //    }
            //}
        }
    }
}