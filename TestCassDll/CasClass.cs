using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestCassDll.Models;

namespace TestCassDll
{
    public class CasClass
    {
        public List<Product> AllPlu { get { return Plus; } set { } }
        public List<Store> AllStore { get { return Stores; } set { } }
        public Product PluData { get { return PluInfo; } set { } }
        public Store StoreData { get { return StoreInfo; } set { } }



        private List<String> SalesPackData = new List<String>();
        private List<Product> Plus = new List<Product>();
        private List<Store> Stores = new List<Store>();
        private int ProcessStatus;
        private byte[] Buffer = new byte[1024];
        private Product PluInfo;
        private Store StoreInfo;
        private Sales SalesRowData;
        private static PropertyInfo[] ProductProperties = typeof(Product).GetProperties();
        private static PropertyInfo[] StoreProperties = typeof(Store).GetProperties();


        public CasClass()
        {
            PluInfo = new Product();
            StoreInfo = new Store();
            SalesRowData = new Sales();
            ProcessStatus = 0;
        }

        private byte[] RequestPluInfo = new byte[]
        {
            0x52,0x30,0x32,0x46,0x30,0x31,0x30,0x30,0x30,0x30,0x30,0x31,0x2c,0x30,0x30,0x0a,
        };
        private byte[] RequestStoreInfo = new byte[]
        {
           0x52,0x33,0x33,0x46,0x30,0x31,0x2c,0x30,0x30,0x30,0x31,0x0a
        };
        private string[] SalesDelimiters = {
                            "1",
                            "1",
                            "1",
                            "1",
                            "4",
                            "4",
                            "4",
                            "2",
                            "2",
                            "4",
                            "4",
                            "4",
                            "4",
                            "2",
                            "1",
                            "1",
                            "1",
                            "1",
                            "1",
                            "1",
                            "1",
                            "1",
                            "1",
                            "1",
                            "20",
                            "20",
                            "1",
                            "1",
                            "55",
                          };

        private void ArrangePackDataType(int Type)
        {
            SalesRowData.Normal = false;
            SalesRowData.Prepack = false;
            SalesRowData.Self_Service = false;
            SalesRowData.Plu_Data = false;
            SalesRowData.Ticket_Data = false;
            switch (Type)
            {
                case 0x00:
                    SalesRowData.Normal = true;
                    break;
                case 0x01:
                    SalesRowData.Prepack_Data = true;
                    break;
                case 0x02:
                    SalesRowData.Self_Service = true;
                    break;
                case 0x20:
                    SalesRowData.Plu_Data = true;
                    break;
                case 0x40:
                    SalesRowData.Ticket_Data = true;
                    break;
            }
        }

        private void ArrangePackSaleType(int Type)
        {
            SalesRowData.Negative_Sale = false;
            SalesRowData.Return = false;
            SalesRowData.Void = false;
            SalesRowData.Prepack = false;
            SalesRowData.Label = false;
            SalesRowData.Override = false;
            SalesRowData.Add = false;
            SalesRowData.NoVoid = false;
            switch (Type)
            {
                case 0x01:
                    SalesRowData.Negative_Sale = true;
                    break;
                case 0x02:
                    SalesRowData.Return = true;
                    break;
                case 0x04:
                    SalesRowData.Void = true;
                    break;
                case 0x08:
                    SalesRowData.Prepack = true;
                    break;
                case 0x10:
                    SalesRowData.Label = true;
                    break;
                case 0x20:
                    SalesRowData.Override = true;
                    break;
                case 0x40:
                    SalesRowData.Add = true;
                    break;
                case 0x80:
                    SalesRowData.NoVoid = true;
                    break;
            }
        }

        private string MakeStr1256(int Number)
        {
            byte[] intBytes = BitConverter.GetBytes(Number);
            return Encoding.GetEncoding(1256).GetString(intBytes, 0, intBytes.Length);
        }
        private string MakeStr1256(short Number)
        {
            byte[] intBytes = BitConverter.GetBytes(Number);
            return Encoding.GetEncoding(1256).GetString(intBytes, 0, intBytes.Length);
        }
        private string MakeStr1256(byte Number)
        {
            byte[] intBytes = new byte[1];
            intBytes[0] = Number;
            return Encoding.GetEncoding(1256).GetString(intBytes, 0, intBytes.Length);
        }
        private int ConvertEncoding1256ToDecimal(string EncodeData)
        {
            byte[] EncodeArray = Encoding.GetEncoding(1256).GetBytes(EncodeData);
            Array.Reverse(EncodeArray, 0, EncodeArray.Length);
            string HexStr = BitConverter.ToString(EncodeArray).Replace("-", "");
            return Int32.Parse(HexStr, System.Globalization.NumberStyles.HexNumber);
        }
        private void ArrangeSalesData(string InputStr)
        {
            int ByteNumber;
            int index = 0;
            String SplitStr = null;
            foreach (string CountStr in SalesDelimiters)
            {
                ByteNumber = Int32.Parse(CountStr);
                SplitStr = InputStr.Substring(index, ByteNumber);
                SalesPackData.Add(SplitStr);
                index += ByteNumber;

            }
        }


        private int ReadSomePlu(string server, int port, bool breakOnSpecialPlu, int Plu_No)
        {
            string Tempstring;
            int PluCounter;
            bool DataAvailabel = true;
            bool IsDataTableEmpty = true;


            for (PluCounter = 1; DataAvailabel == true; PluCounter++)
            {
                try
                {
                    var client = new TcpClient();
                    var result = client.BeginConnect(server, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!success)
                        return -1; // client is null !!!
                    else
                    {
                        NetworkStream stream = client.GetStream();
                        Tempstring = PluCounter.ToString("X6");
                        ASCIIEncoding.ASCII.GetBytes(Tempstring, 0, 6, RequestPluInfo, 6);
                        stream.Write(RequestPluInfo, 0, RequestPluInfo.Length);
                        String responseData = String.Empty;
                        int BytesCount = stream.Read(Buffer, 0, Buffer.Length);
                        ProcessStatus = ProcessRecivedPluData(Buffer, BytesCount);
                        if (ProcessStatus == 1)
                        {
                            if (breakOnSpecialPlu == true)
                            {
                                if (PluInfo.PLU_No == Plu_No)
                                {
                                    DataAvailabel = false;
                                    IsDataTableEmpty = false;
                                }
                            }
                            else
                            {
                                Plus.Add(PluInfo);
                                IsDataTableEmpty = false;
                            }
                        }
                        else
                            DataAvailabel = false;

                        stream.Close();
                        client.Close();
                    }
                }
                catch (ArgumentNullException)
                {
                    return -2; // ArgumentNullException !!!
                }
                catch (SocketException)
                {
                    return -3; // SocketException !!!
                }
            }
            if (IsDataTableEmpty == true)
            {
                foreach (PropertyInfo info in ProductProperties)
                    info.SetValue(PluInfo, null);
                return 2; // succeed : scale has no data
            }
            return 1; // succeed : scale has data 
        }
        public int ReadPlu(string server, int port, int Plu_No)
        {
            return ReadSomePlu(server, port, true, Plu_No);
        }
        public int ReadAllPlus(string server, int port)
        {
            return ReadSomePlu(server, port, false, 0);
        }
        public int WritePlu(string server, int port)
        {
            string PackData = "W02A";
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(server, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                int PackLenght = 0;
                if (!success)
                    return -1; // client is null !!!
                else
                {
                    NetworkStream stream = client.GetStream();

                    PackData += PluData.PLU_No.ToString("X5");
                    PackData += ",";
                    PackData += PluData.DepartmentNo.ToString("X2");
                    PackData += "L";
                    PackData += "0000";
                    PackData += ":";
                    PackData = PackData + "F=01.57,2:" + MakeStr1256((short)PluData.DepartmentNo);
                    PackData = PackData + "F=02.4C,4:" + MakeStr1256(PluData.PLU_No);
                    PackData = PackData + "F=04.4D,1:" + MakeStr1256((byte)PluData.PLU_Type);
                    PackData = PackData + "F=05.42,1:" + MakeStr1256((byte)PluData.Unit_Weight);
                    PackData = PackData + "F=06.4C,4:" + MakeStr1256(PluData.Unit_Price);
                    PackData = PackData + "F=08.42,1:" + MakeStr1256((byte)PluData.TaxCode);
                    PackData = PackData + "F=09.57,2:" + MakeStr1256((short)PluData.Group_No);
                    PackData = PackData + "F=0B.4C,4:" + MakeStr1256(Int32.Parse(PluData.Itemcode));
                    PackData = PackData + "F=0C.42,1:" + MakeStr1256((byte)PluData.Tare_No);
                    PackData = PackData + "F=0D.4C,4:" + MakeStr1256(PluData.TareValue);
                    PackData = PackData + "F=50.57,2:" + MakeStr1256((short)PluData.Label_No);
                    PackData = PackData + "F=51.57,2:" + MakeStr1256((short)PluData.Ax_Label_No);
                    PackData = PackData + "F=55.57,2:" + MakeStr1256((short)PluData.Barcode_No);
                    PackData = PackData + "F=56.57,2:" + MakeStr1256((short)PluData.Barcode2_No);
                    PackData = PackData + "F=5A.42,1:" + MakeStr1256((byte)PluData.SaleMessage_No);
                    PackData = PackData + "F=5B.42,4:" + MakeStr1256(PluData.Special_Price);
                    PackData = PackData + "F=0A.53," + PluData.Name1.Length.ToString("X1") + ":" + PluData.Name1;
                    byte[] EncodeArray = Encoding.GetEncoding(1256).GetBytes(PackData);
                    PackLenght = EncodeArray.Length - 18;
                    Array.Resize(ref EncodeArray, EncodeArray.Length + 1);
                    byte Chcksum = EncodeArray[18];
                    for (int Counter = 19; Counter < EncodeArray.Length; Counter++)
                        Chcksum ^= EncodeArray[Counter];

                    EncodeArray[EncodeArray.Length - 1] = Chcksum;

                    PackLenght = EncodeArray.Length - 19;

                    string LenghtStr = PackLenght.ToString("X4");

                    ASCIIEncoding.ASCII.GetBytes(LenghtStr, 0, 4, EncodeArray, 13);

                    stream.Write(EncodeArray, 0, EncodeArray.Length);
                    stream.Close();
                    client.Close();
                }
            }
            catch (ArgumentNullException)
            {
                return -2; // ArgumentNullException !!!
            }
            catch (SocketException)
            {
                return -3; // SocketException !!!
            }
            return 1;
        }


        private int ReadSomeStore(string server, int port, bool breakOnSpecialStore, int Store_No)
        {
            string Tempstring;
            int StoreCounter;
            bool DataAvailabel = true;
            bool IsDataTableEmpty = true;


            for (StoreCounter = 1; DataAvailabel == true; StoreCounter++)
            {
                try
                {
                    var client = new TcpClient();
                    var result = client.BeginConnect(server, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!success)
                        return -1; // client is null !!!
                    else
                    {
                        NetworkStream stream = client.GetStream();
                        Tempstring = StoreCounter.ToString("X4");
                        ASCIIEncoding.ASCII.GetBytes(Tempstring, 0, 4, RequestStoreInfo, 7);
                        stream.Write(RequestStoreInfo, 0, RequestStoreInfo.Length);
                        String responseData = String.Empty;
                        int BytesCount = stream.Read(Buffer, 0, Buffer.Length);
                        ProcessStatus = ProcessRecivedStoreData(Buffer, BytesCount);
                        if (ProcessStatus == 1)
                        {

                            if (breakOnSpecialStore == true)
                            {
                                if (StoreCounter == Store_No)
                                {
                                    DataAvailabel = false;
                                    IsDataTableEmpty = false;
                                    StoreInfo.Code = StoreCounter;
                                }
                            }
                            else
                            {
                                StoreInfo.Code = StoreCounter;
                                Stores.Add(StoreInfo);
                                IsDataTableEmpty = false;
                            }
                        }
                        else
                            DataAvailabel = false;


                        stream.Close();
                        client.Close();
                    }
                }
                catch (ArgumentNullException)
                {
                    return -2; // ArgumentNullException !!!
                }
                catch (SocketException)
                {
                    return -3; // SocketException !!!
                }
            }
            if (IsDataTableEmpty == true)
            {
                foreach (PropertyInfo info in StoreProperties)
                    info.SetValue(StoreInfo, null);
                return 2; // succeed : scale has no data
            }
            return 1; // succeed : scale has data 
        }
        public int ReadStore(string server, int port, int Store_No)
        {
            return ReadSomeStore(server, port, true, Store_No);
        }
        public int ReadAllStores(string server, int port)
        {
            return ReadSomeStore(server, port, false, 0);
        }
        public int WriteStore(string server, int port)
        {
            string PackData = "W32F01,";
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(server, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                int PackLenght = 0;
                if (!success)
                    return -1; // client is null !!!
                else
                {
                    NetworkStream stream = client.GetStream();

                    PackData += StoreData.Code.ToString("X4");
                    PackData += "L000:";
                    PackData = PackData + "P=" + StoreData.Name.Length.ToString("X2") + "." + StoreData.Name;
                    PackData = PackData + "T=" + StoreData.Phone.Length.ToString("X2") + "." + StoreData.Phone;
                    PackData = PackData + "S=" + StoreData.Description.Length.ToString("X2") + "." + StoreData.Description;
                    byte[] EncodeArray = Encoding.GetEncoding(1256).GetBytes(PackData);
                    PackLenght = EncodeArray.Length - 16;
                    Array.Resize(ref EncodeArray, EncodeArray.Length + 1);
                    byte Chcksum = EncodeArray[16];
                    for (int Counter = 17; Counter < EncodeArray.Length; Counter++)
                        Chcksum ^= EncodeArray[Counter];

                    EncodeArray[EncodeArray.Length - 1] = Chcksum;

                    PackLenght = EncodeArray.Length - 17;

                    string LenghtStr = PackLenght.ToString("X3");

                    ASCIIEncoding.ASCII.GetBytes(LenghtStr, 0, 3, EncodeArray, 12);

                    stream.Write(EncodeArray, 0, EncodeArray.Length);
                    stream.Close();
                    client.Close();
                }
            }
            catch (ArgumentNullException)
            {
                return -2; // ArgumentNullException !!!
            }
            catch (SocketException)
            {
                return -3; // SocketException !!!
            }
            return 1;
        }
        public int SendAck(string server, int port)
        {
            string PackData = "i00F070,02L02B:^=";
            try
            {
                var client = new TcpClient();
                var result = client.BeginConnect(server, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                int PackLenght = 0;
                if (!success)
                    return -1; // client is null !!!
                else
                {
                    NetworkStream stream = client.GetStream();

                    PackData += SalesRowData.Scale_ID.ToString("X2");
                    PackData += ".*=";
                    PackData += SalesRowData.Department_No.ToString("X2");
                    PackData += ".$=";
                    PackData += SalesRowData.Lock_Info.ToString("X1");
                    PackData += ".&=";
                    var localEndPoint = client.Client.LocalEndPoint as IPEndPoint;
                    var localAddress = localEndPoint.Address;
                    var localPort = localEndPoint.Port;
                    byte[] bytes = localAddress.GetAddressBytes();
                    PackData+= string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", bytes[0], bytes[1], bytes[2], bytes[3]);
                    PackData += ".@=";
                    PackData += SalesRowData.Pack_Port.ToString("X4");
                    PackData += ".?=";
                    PackData += SalesRowData.Scale_Service_Type.ToString("X1");
                    PackData += ".T=";
                    PackData += SalesRowData.Row.ToString("X4") + ".";
                    byte[] EncodeArray = Encoding.GetEncoding(1256).GetBytes(PackData);
                    PackLenght = EncodeArray.Length - 15;
                    Array.Resize(ref EncodeArray, EncodeArray.Length + 1);
                    byte Chcksum = EncodeArray[15];
                    for (int Counter = 16; Counter < EncodeArray.Length; Counter++)
                        Chcksum ^= EncodeArray[Counter];

                    EncodeArray[EncodeArray.Length - 1] = Chcksum;

                    PackLenght = EncodeArray.Length - 16;

                    string LenghtStr = PackLenght.ToString("X3");

                    ASCIIEncoding.ASCII.GetBytes(LenghtStr, 0, 3, EncodeArray, 11);

                    stream.Write(EncodeArray, 0, EncodeArray.Length);
                    stream.Close();
                    client.Close();
                }
            }
            catch (ArgumentNullException)
            {
                return -2; // ArgumentNullException !!!
            }
            catch (SocketException)
            {
                return -3; // SocketException !!!
            }
            return 1;
        }

        public int ReadSale(string host, int port)
        {

            int SalesBytesReceivd = 0;
            NetworkStream stream;
            TcpListener server = null;
            ProcessStatus = 0;
            try
            {
                IPAddress localAddr = IPAddress.Parse(host);
                server = new TcpListener(localAddr, port);
                server.Start();
                while (ProcessStatus == 0)
                {

                    TcpClient client = server.AcceptTcpClient();
                    stream = client.GetStream();
                    SalesBytesReceivd = stream.Read(Buffer, 0, Buffer.Length);
                    if (SalesBytesReceivd > 0)
                        ProcessStatus = ProcessRecivedSalesData(Buffer, SalesBytesReceivd);
                    client.Close();
                    stream.Close();
                }
            }
            catch (SocketException)
            {
                return -1;
            }
            finally
            {
                server.Stop();
            }
            return ProcessStatus;
        }
        private short ProcessRecivedStoreData(byte[] data, int BytesCount)
        {

            int ChecksumIndex = 16;
            byte CalculatedChecksum = 0;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 7) == "R33:E99")
                return -1;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 7) != "W32F01,")
                return -2;
            if (BytesCount < ChecksumIndex)
                return -3;
            CalculatedChecksum = data[16];
            for (int PackCounter = ChecksumIndex + 1; PackCounter < BytesCount - 1; PackCounter++)
                CalculatedChecksum ^= data[PackCounter];
            if (CalculatedChecksum != data[BytesCount - 1])
                return -4;
            else
            {
                string[] Delimiters = {
                            ":",
                            "=",
                            "P=",
                            "S=",
                            "T=",
                            "."
                          };
                string BasePack = Encoding.GetEncoding(1256).GetString(data, 0, BytesCount - 1);
                var SplitedData = BasePack.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);

                StoreInfo.Scale_ID = Int32.Parse(SplitedData[2], System.Globalization.NumberStyles.HexNumber);
                StoreInfo.Lock_Info = (byte)(Int32.Parse(SplitedData[6], System.Globalization.NumberStyles.HexNumber));
                StoreInfo.Pack_IP = String.Format("{0}.{1}.{2}.{3}",
                    int.Parse(SplitedData[8].Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedData[8].Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedData[8].Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedData[8].Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                StoreInfo.Pack_Port = Int32.Parse(SplitedData[10], System.Globalization.NumberStyles.HexNumber);
                StoreInfo.Scale_Service_Type = (byte)Int32.Parse(SplitedData[12], System.Globalization.NumberStyles.HexNumber);
                StoreInfo.Name = SplitedData[14];
                StoreInfo.Phone = SplitedData[16];
                StoreInfo.Description = SplitedData[18];
            }
            return 1;
        }
        private short ProcessRecivedPluData(byte[] data, int BytesCount)
        {

            int ChecksumIndex = 18;
            byte CalculatedChecksum = 0;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 13) == "W02AF4240,00L")
                return -1;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 4) != "W02A")
                return -2;
            if (BytesCount < ChecksumIndex)
                return -3;
            CalculatedChecksum = data[18];
            for (int PackCounter = ChecksumIndex + 1; PackCounter < BytesCount - 1; PackCounter++)
                CalculatedChecksum ^= data[PackCounter];
            if (CalculatedChecksum != data[BytesCount - 1])
                return -4;
            else
            {
                string[] CountDelimiter = {
                            "F=",
                          };
                string[] ConfigDelimiters = {
                            "W02A",
                            "L",
                            ",",
                            ".",
                            "=",
                          };
                string[] DataDelimiters = {
                            "F=",
                            ":",
                          };
                string BasePack = Encoding.GetEncoding(1256).GetString(data, 0, BytesCount - 1);
                var DataCount = BasePack.Split(CountDelimiter, StringSplitOptions.RemoveEmptyEntries).Count() - 1;
                var SplitedConfigs = BasePack.Split(ConfigDelimiters, StringSplitOptions.RemoveEmptyEntries);
                var SplitedData = BasePack.Split(DataDelimiters, StringSplitOptions.RemoveEmptyEntries);

                PluInfo.Scale_ID = Int32.Parse(SplitedConfigs[3], System.Globalization.NumberStyles.HexNumber);
                PluInfo.Lock_Info = (byte)(Int32.Parse(SplitedConfigs[7], System.Globalization.NumberStyles.HexNumber));
                PluInfo.Pack_IP = String.Format("{0}.{1}.{2}.{3}",
                    int.Parse(SplitedConfigs[9].Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedConfigs[9].Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedConfigs[9].Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedConfigs[9].Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                PluInfo.Pack_Port = Int32.Parse(SplitedConfigs[11], System.Globalization.NumberStyles.HexNumber);
                PluInfo.Scale_Service_Type = (byte)Int32.Parse(SplitedConfigs[13], System.Globalization.NumberStyles.HexNumber);
                PluInfo.Row = Int32.Parse(SplitedConfigs[15], System.Globalization.NumberStyles.HexNumber);
                string SpFunc;
                string[] SplitedFunc;
                for (int LoopCnt = 0, DataCounter = 3; LoopCnt < DataCount; LoopCnt++, DataCounter += 2)
                {
                    SpFunc = SplitedData[DataCounter - 1];
                    SplitedFunc = SpFunc.Split(',');
                    switch (SplitedFunc[0].ToUpper())
                    {
                        case "01.57":
                            PluInfo.DepartmentNo = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "0A.53":
                            PluInfo.Name1 = SplitedData[DataCounter];
                            break;
                        case "0B.4C":
                            PluInfo.Itemcode = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]).ToString();
                            break;
                        case "06.4C":
                            PluInfo.Unit_Price = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "04.4D":
                            PluInfo.PLU_Type = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "02.4C":
                            PluInfo.PLU_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "05.42":
                            PluInfo.Unit_Weight = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "09.57":
                            PluInfo.Group_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "0D.4C":
                            PluInfo.TareValue = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "0C.42":
                            PluInfo.Tare_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "08.42":
                            PluInfo.TaxCode = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "51.57":
                            PluInfo.Ax_Label_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "56.57":
                            PluInfo.Barcode2_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "5A.42":
                            PluInfo.SaleMessage_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "5B.42":
                            PluInfo.Special_Price = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "55.57":
                            PluInfo.Barcode_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                        case "50.57":
                            PluInfo.Label_No = ConvertEncoding1256ToDecimal(SplitedData[DataCounter]);
                            break;
                    }
                }

            }
            return 1;
        }
        private short ProcessRecivedSalesData(byte[] data, int BytesCount)
        {

            int ChecksumIndex = 15;
            byte CalculatedChecksum = 0;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 11) != "i00F070,40L")
                return -1;
            if (BytesCount < ChecksumIndex)
                return -2;
            CalculatedChecksum = data[15];
            for (int PackCounter = ChecksumIndex + 1; PackCounter < BytesCount - 1; PackCounter++)
                CalculatedChecksum ^= data[PackCounter];
            if (CalculatedChecksum != data[BytesCount - 1])
                return -3;
            else
            {
                string[] Delimiters = { ":","=","." };
                string BasePack = Encoding.GetEncoding(1256).GetString(data, 0, BytesCount - 1);
                var SplitedData = BasePack.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                ArrangeSalesData(SplitedData[15]);
                SalesRowData.Scale_ID = Int32.Parse(SplitedData[2], System.Globalization.NumberStyles.HexNumber);
                SalesRowData.Lock_Info = (byte)(Int32.Parse(SplitedData[6], System.Globalization.NumberStyles.HexNumber));
                SalesRowData.Pack_IP = String.Format("{0}.{1}.{2}.{3}",
                    int.Parse(SplitedData[8].Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedData[8].Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedData[8].Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedData[8].Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                SalesRowData.Pack_Port = Int32.Parse(SplitedData[10], System.Globalization.NumberStyles.HexNumber);
                SalesRowData.Scale_Service_Type = (byte)Int32.Parse(SplitedData[12], System.Globalization.NumberStyles.HexNumber);
                SalesRowData.Row = Int32.Parse(SplitedData[14], System.Globalization.NumberStyles.HexNumber);
                ArrangePackDataType(ConvertEncoding1256ToDecimal(SalesPackData[0]));
                SalesRowData.Scale_ID = ConvertEncoding1256ToDecimal(SalesPackData[1]);
                SalesRowData.PLU_Type = (byte)ConvertEncoding1256ToDecimal(SalesPackData[2]);
                SalesRowData.Department_No = (byte)ConvertEncoding1256ToDecimal(SalesPackData[3]);
                SalesRowData.PLU_No = ConvertEncoding1256ToDecimal(SalesPackData[4]);
                SalesRowData.Item_Code = ConvertEncoding1256ToDecimal(SalesPackData[5]);
                SalesRowData.Weight = ConvertEncoding1256ToDecimal(SalesPackData[6]);
                SalesRowData.Qty = (short)ConvertEncoding1256ToDecimal(SalesPackData[7]);
                SalesRowData.Pcs = (short)ConvertEncoding1256ToDecimal(SalesPackData[8]);
                SalesRowData.Unit_Price = (short)ConvertEncoding1256ToDecimal(SalesPackData[9]);
                SalesRowData.Total_Price = ConvertEncoding1256ToDecimal(SalesPackData[10]);
                SalesRowData.Discount_Price = (short)ConvertEncoding1256ToDecimal(SalesPackData[11]);
                SalesRowData.Scale_Transaction_Counter = ConvertEncoding1256ToDecimal(SalesPackData[12]);
                SalesRowData.Ticket_Number = (short)ConvertEncoding1256ToDecimal(SalesPackData[13]);
                ArrangePackSaleType(ConvertEncoding1256ToDecimal(SalesPackData[14]));
                SalesRowData.CurrentDate_Year = (short)ConvertEncoding1256ToDecimal(SalesPackData[15]);
                SalesRowData.CurrentDate_Month = (short)ConvertEncoding1256ToDecimal(SalesPackData[16]);
                SalesRowData.CurrentDate_Day = (short)ConvertEncoding1256ToDecimal(SalesPackData[17]);
                SalesRowData.CurrentTime_Hour = (short)ConvertEncoding1256ToDecimal(SalesPackData[18]);
                SalesRowData.CurrentTime_Min= (short)ConvertEncoding1256ToDecimal(SalesPackData[19]);
                SalesRowData.CurrentTime_Second = (short)ConvertEncoding1256ToDecimal(SalesPackData[20]);
                SalesRowData.SaleDate_Year = (short)ConvertEncoding1256ToDecimal(SalesPackData[21]);
                SalesRowData.SaleDate_Month = (short)ConvertEncoding1256ToDecimal(SalesPackData[22]);
                SalesRowData.SaleDate_Day = (short)ConvertEncoding1256ToDecimal(SalesPackData[23]);
                SalesRowData.Barcode = SalesPackData[24];
                SalesRowData.Trace_Code = SalesPackData[25];
                SalesRowData.Current_Ticket_Sale_Order = (byte)ConvertEncoding1256ToDecimal(SalesPackData[26]);
                SalesRowData.Plu_Name = SalesPackData[28];
            }
            return 1;
        }
    }
}
