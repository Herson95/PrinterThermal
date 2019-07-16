namespace PrinterThermal.DependencyServices
{
    using System.Threading.Tasks;
    public interface IPrinterService
    {
        Task Test(string ip, int port, string addressBluetooth, int fontSize = 0, string type = "1");
    }
}
