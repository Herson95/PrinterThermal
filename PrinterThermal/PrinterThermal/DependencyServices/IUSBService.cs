namespace PrinterThermal.DependencyServices
{
    using System.Collections.Generic;
    public interface IUSBService
    {
        IList<string> GetDeviceList();

        void ConnectAndSend(int productId, int vendorId);

    }
}
