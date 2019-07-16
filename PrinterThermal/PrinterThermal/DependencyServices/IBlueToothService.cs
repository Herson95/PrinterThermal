namespace PrinterThermal.DependencyServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBlueToothService
    {
        IList<string> GetDeviceList();

        //Task Print(string deviceName, string text);
    }
}
