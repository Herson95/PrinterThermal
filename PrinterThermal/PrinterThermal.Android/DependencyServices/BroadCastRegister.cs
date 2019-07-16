
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.Widget;

namespace PrinterThermal.Droid.DependencyServices
{
    //[BroadcastReceiver(Enabled = true)]
    //[IntentFilter(new[] { "android.hardware.usb.action.USB_PERMISSION" })]
    //public class BroadCastRegister : BroadcastReceiver
    //{
    //    protected static string ACTION_USB_PERMISSION = "android.hardware.usb.action.USB_PERMISSION";
    //    public override void OnReceive(Context context, Intent intent)
    //    {
    //        string action = intent.Action;
    //        if (ACTION_USB_PERMISSION.Equals(action))
    //        {
    //            AndroidUSBService.mDevice = (UsbDevice)intent.GetParcelableExtra(UsbManager.ExtraDevice);
    //            if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
    //            {
    //                if (AndroidUSBService.mDevice != null)
    //                {
    //                    Toast.MakeText(MainActivity.Context,"Connected",ToastLength.Long).Show();
    //                }
    //            }
    //            else
    //            {
                   
    //            }
    //        }
    //    }
    //}
}