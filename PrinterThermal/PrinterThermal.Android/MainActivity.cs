using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Hardware.Usb;
using Android;
using PrinterThermal.Droid.DependencyServices;
using Android.Bluetooth;

namespace PrinterThermal.Droid
{
    [Activity(Label = "PrinterThermal", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static string ACTION_USB_PERMISSION = "android.hardware.usb.action.USB_PERMISSION";

        public static Context Context { get;  set; }
        public static UsbManager UsbManager { get;  set; }
        public static UsbDevice UsbDevice { get;  set; }
        public static PendingIntent PendingIntent { get;  set; }

        //private const int LocationPermissionsRequestCode = 1000;

        private static readonly string[] LocationPermissions =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.BluetoothPrivileged,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Context = this;
            UsbManager = (UsbManager)GetSystemService(UsbService);
            PendingIntent = PendingIntent.GetBroadcast(Context, 0, new Intent(ACTION_USB_PERMISSION), 0);
            //CreateConnectionUSB();
            CheckPermissions();

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

        public static void CreateConnectionUSB()
        {
            PendingIntent = PendingIntent.GetBroadcast(Context, 0, new Intent(ACTION_USB_PERMISSION), 0);
            IntentFilter filter = new IntentFilter(ACTION_USB_PERMISSION);
            Context.RegisterReceiver(new UsbReceiver(), filter);
        }

        private void CheckPermissions()
        {
            //var coarseLocationPermissionGranted =
            //   CheckSelfPermission(Manifest.Permission.AccessCoarseLocation);
            //var fineLocationPermissionGranted =
            //   CheckSelfPermission(Manifest.Permission.AccessFineLocation);

            //if (coarseLocationPermissionGranted != Permission.Denied ||
            //    fineLocationPermissionGranted == Permission.Denied)
            //    RequestPermissions(LocationPermissions, LocationPermissionsRequestCode);

            bool minimumPermissionsGranted = true;

            foreach (string permission in LocationPermissions)
            {
                if (CheckSelfPermission(permission) != Permission.Granted) minimumPermissionsGranted = false;
            }

            // If one of the minimum permissions aren't granted, we request them from the user
            if (!minimumPermissionsGranted) RequestPermissions(LocationPermissions, 0);
        }
    }
}