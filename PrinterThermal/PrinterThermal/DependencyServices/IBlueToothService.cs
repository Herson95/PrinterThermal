namespace PrinterThermal.DependencyServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBlueToothService
    {
        Task<IList<string>> GetPairedDevice();

       IList<string> GetNoPairedDevice();

       Task<bool> ScanDeviceNoPaired();

       Task<bool> PairedDevice(string deviceAddress);
    }
}
