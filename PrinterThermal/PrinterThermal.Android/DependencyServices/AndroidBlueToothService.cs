using PrinterThermal.Droid.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidBlueToothService))]
namespace PrinterThermal.Droid.DependencyServices
{
    using System.Collections.Generic;
    using System.Linq;
    using Android.Bluetooth;
    using PrinterThermal.DependencyServices;

    public class AndroidBlueToothService : IBlueToothService
    {
        public IList<string> GetDeviceList()
        {
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                if (bluetoothAdapter == null)
                {
                    return new List<string>();
                }
                var btdevice = bluetoothAdapter?.BondedDevices.Select(i => i.Name + " " + i.Address).ToList();
                return btdevice;
            }
        }

        //public async Task Print(string deviceName, string text)
        //{
        //    var address = deviceName.Split(" ").LastOrDefault();
        //    using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
        //    {
        //        BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
        //                                  where bd?.Address == address
        //                                  select bd).FirstOrDefault();
        //        try
        //        {
        //            using (BluetoothSocket bluetoothSocket = device?.
        //                CreateRfcommSocketToServiceRecord(
        //                UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
        //            {
        //                await bluetoothSocket.ConnectAsync();

        //                await Task.Run(() =>
        //                {
        //                    pw = new PrintWriter(bluetoothSocket.OutputStream, true);
        //                    EscInit();
        //                    SetJustification(1);

        //                    SetBold(true);
        //                    StoreString("PRINTER TEST \n\n");
        //                    SetBold(false);

        //                    PrintStorage();

        //                    FeedAndCut(4);

        //                    pw.Flush();
        //                    pw.Close();
        //                    pw.Dispose();


        //                    bluetoothSocket.Close();
        //                    bluetoothSocket.Dispose();
        //                });
        //            }
        //        }
        //        catch (Exception exp)
        //        {
        //            throw exp;
        //        }
        //    }
        //}

    }
}