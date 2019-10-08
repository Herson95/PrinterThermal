namespace PrinterThermal.DependencyServices
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUSBService
    {
        IList<string> GetDeviceList();

        Task ConnectAndSend(int productId, int vendorId);

        void CreateConnection();

    }
}
