using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Hardware.Usb;

namespace PrinterThermal.Droid
{
    [Activity(Label = "PrinterThermal", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected static string ACTION_USB_PERMISSION = "android.hardware.usb.action.USB_PERMISSION";

        public static Context Context { get;  set; }
        public static UsbManager UsbManager { get;  set; }
        public static UsbDevice UsbDevice { get;  set; }
        public static PendingIntent PendingIntent { get;  set; }
       


        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Context = this;
            UsbManager = (UsbManager)Context.GetSystemService(UsbService);
            PendingIntent = PendingIntent.GetBroadcast(Context, 0, new Intent(ACTION_USB_PERMISSION), 0);

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public static void CreateConnection()
        {
            if (UsbManager.DeviceList.Count != 0)
            {
                foreach (UsbDevice device in UsbManager.DeviceList.Values)
                {
                    UsbManager.RequestPermission(device, PendingIntent);
                }
            }
        }
    }
}