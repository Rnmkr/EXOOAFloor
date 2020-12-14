using System.IO;

namespace EXOOAFloor.Helpers
{
    public class ServerFiles
    {
        public string ServerLogsBaseFolder { get; private set; }
        public string OrderLogsBaseFolder { get; private set; }
        public string MasterConfigFilePath { get; private set; }
        public string SerialNumberLogsFolder { get; private set; }
        public string TestLogFile { get; private set; }
        public string BitLogFile { get; private set; }
        public string EmbaladoOkFile { get; private set; }
        public string IdealFile { get; private set; }
        public string BinFile { get; private set; }
        public string XmlFile { get; private set; }
        public string PixelTrashFile { get; private set; }
        public string TestOkFile { get; private set; }
        public ServerFiles(string serialNumber)
        {
            ServerLogsBaseFolder = @"\\bubba\ea2100dc89ae9fe21fa9b08ab1bf18662dca1e53a3eebd7d03afebcaf5d57515$";

            OrderLogsBaseFolder = Path.Combine(serialNumber.Substring(0, 1), serialNumber.Substring(1, 3),
                serialNumber.Substring(4, 4));

            MasterConfigFilePath = Path.Combine(OrderLogsBaseFolder, "Masterconfig.cfg");

            SerialNumberLogsFolder = Path.Combine(OrderLogsBaseFolder, serialNumber.Substring(8, 5));

            TestLogFile = Path.Combine(SerialNumberLogsFolder, (serialNumber + ".txt"));

            BitLogFile = Path.Combine(SerialNumberLogsFolder, "bit.log");

            EmbaladoOkFile = Path.Combine(SerialNumberLogsFolder, "Embalado-*.OK");

            IdealFile = Path.Combine(SerialNumberLogsFolder, "Ideal.cfg");

            BinFile = Path.Combine(SerialNumberLogsFolder, "OA3.bin");

            XmlFile = Path.Combine(SerialNumberLogsFolder, "OA3.xml");

            PixelTrashFile = Path.Combine(SerialNumberLogsFolder, "pixel.trash");

            TestOkFile = Path.Combine(SerialNumberLogsFolder, "TEST-*.OK");
        }
    }
}
