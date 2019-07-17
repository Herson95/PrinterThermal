namespace PrinterThermal.DependencyServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBlueToothService
    {
       IList<string> GetPairedDevice();

       IList<string> GetNoPairedDevice();

       Task<bool> ScanDeviceNoPaired();
    }
}
