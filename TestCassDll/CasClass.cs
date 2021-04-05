﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestCassDll.Models;

namespace TestCassDll
{
    public class CasClass
    {
        public List<Product> AllPlu { get { return Plus; } set { } }
        public ReceivedConfig ScaleConfigs { get { return _receivedConfig; } set { } }
        public Product PluData { get { return PluInfo; } set { } }

        private List<Product> Plus = new List<Product>();
        private int ProcessStatus;
        private byte[] Buffer = new byte[1024];
        private Product PluInfo;
        private ReceivedConfig _receivedConfig;

        public CasClass()
        {
            PluInfo = new Product();
            _receivedConfig = new ReceivedConfig();
            ProcessStatus = 0;
        }

        private byte[] RequestPluInfo = new byte[]
        {
            0x52,0x30,0x32,0x46,0x30,0x31,0x30,0x30,0x30,0x30,0x30,0x31,0x2c,0x30,0x30,0x0a,
        };

        public int ReadPlu(string server, int port, int Plu_No)
        {
            return ReadAllPlus(server, port, true, Plu_No);
        }

        public string MakeStr1256(int Number)
        {
            byte[] intBytes = BitConverter.GetBytes(Number);
            return Encoding.GetEncoding(1256).GetString(intBytes, 0, intBytes.Length);
        }
        public string MakeStr1256(short Number)
        {
            byte[] intBytes = BitConverter.GetBytes(Number);
            return Encoding.GetEncoding(1256).GetString(intBytes, 0, intBytes.Length);
        }
        public string MakeStr1256(byte Number)
        {
            byte[] intBytes = new byte[1];
            intBytes[0] = Number;
            return Encoding.GetEncoding(1256).GetString(intBytes, 0, intBytes.Length);
        }


        public int WritePlu(string server, int port)
        {
            string PackData="W02A";
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
                    PackData = PackData + "F=0A.53," + PluData.Name1.Length.ToString("X1") + ":"+PluData.Name1;
                    byte[] EncodeArray = Encoding.GetEncoding(1256).GetBytes(PackData);
                    PackLenght = EncodeArray.Length  - 18;
                    Array.Resize(ref EncodeArray, EncodeArray.Length + 1);
                    byte Chcksum = EncodeArray[18];
                    for (int Counter=19;Counter< EncodeArray.Length;Counter++)
                        Chcksum ^= EncodeArray[Counter];

                    EncodeArray[EncodeArray.Length - 1] = Chcksum;

                    PackLenght = EncodeArray.Length - 19;

                    string LenghtStr= PackLenght.ToString("X4");

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
        public int ReadAll(string server, int port)
        {
            return ReadAllPlus(server, port,false,0);
        }

        private int ReadAllPlus(string server, int port,bool breakOnSpecialPlu,int Plu_No)
        {
            string Tempstring;
            int PluCounter;
            bool DataAvailabel = true;


            for (PluCounter = 1; DataAvailabel == true; PluCounter++)
            {
                try
                {
                    var client = new TcpClient();
                    var result = client.BeginConnect(server, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                    if (!success )
                        return -1; // client is null !!!
                    else
                    {
                        NetworkStream stream = client.GetStream();
                        Tempstring = PluCounter.ToString("X6");
                        ASCIIEncoding.ASCII.GetBytes(Tempstring, 0, 6, RequestPluInfo, 6);
                        stream.Write(RequestPluInfo, 0, RequestPluInfo.Length);
                        String responseData = String.Empty;
                        stream.Read(Buffer, 0, Buffer.Length);
                        ProcessStatus = ProcessRecivedPluData(Buffer);
                        if (ProcessStatus == 1)
                        {
                            if (breakOnSpecialPlu == true)
                            {
                               if(PluInfo.PLU_No == Plu_No)
                                    DataAvailabel = false;
                            }
                            else
                                Plus.Add(PluInfo);
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
            if (DataAvailabel == false || ProcessStatus==-1)
            {
                if (Plus.Count() == 0)
                    return 2; // succeed : scale has no data
            }
            return 1; // succeed : scale has data 
        }


        private int ConvertEncoding1256ToDecimal(string EncodeData)
        {
            byte[] EncodeArray = Encoding.GetEncoding(1256).GetBytes(EncodeData);
            Array.Reverse(EncodeArray, 0, EncodeArray.Length);
            string HexStr = BitConverter.ToString(EncodeArray).Replace("-", "");
            return Int32.Parse(HexStr, System.Globalization.NumberStyles.HexNumber);
        }
        private short ProcessRecivedPluData( byte[] data)
        {
            
            int ChecksumIndex = 18;
            byte CalculatedChecksum = 0;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 13) == "W02AF4240,00L")
                return -1;
            if (Encoding.GetEncoding(1256).GetString(data, 0, 4) != "W02A")
                return -2;
            if (data.Length < ChecksumIndex)
                return -3;
            CalculatedChecksum = data[18];
            for (int PackCounter = ChecksumIndex + 1; PackCounter < data.Length - 1; PackCounter++)
                CalculatedChecksum ^= data[PackCounter];
            if (CalculatedChecksum != data[data.Length - 1])
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
                string BasePack = Encoding.GetEncoding(1256).GetString(data, 0, data.Length - 1);
                var DataCount = BasePack.Split(CountDelimiter, StringSplitOptions.RemoveEmptyEntries).Count() - 1;
                var SplitedConfigs = BasePack.Split(ConfigDelimiters, StringSplitOptions.RemoveEmptyEntries);
                var SplitedData = BasePack.Split(DataDelimiters, StringSplitOptions.RemoveEmptyEntries);

                _receivedConfig.ScaleID = Int32.Parse(SplitedConfigs[3], System.Globalization.NumberStyles.HexNumber);
                _receivedConfig.LockInfo = (Int32.Parse(SplitedConfigs[7], System.Globalization.NumberStyles.HexNumber)) > 0 ? true : false;
                _receivedConfig.PackIP = String.Format("{0}.{1}.{2}.{3}",
                    int.Parse(SplitedConfigs[9].Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedConfigs[9].Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedConfigs[9].Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(SplitedConfigs[9].Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
                _receivedConfig.PackPort = Int32.Parse(SplitedConfigs[11], System.Globalization.NumberStyles.HexNumber);
                _receivedConfig.ScaleServiceType = (byte)Int32.Parse(SplitedConfigs[13], System.Globalization.NumberStyles.HexNumber);
                _receivedConfig.TableRow = Int32.Parse(SplitedConfigs[15], System.Globalization.NumberStyles.HexNumber);
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
    }
}
