using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCassDll
{
    class Program
    {

        public static string MakeStr1252(int Value,short Number)
        {
            
            byte[] intBytes = BitConverter.GetBytes(Value);
            byte[] OutArray = new byte[Number];
            //            Array.Reverse(intBytes);
            if (Number > intBytes.Length)
            {
                for (int cnt = 0; cnt < Number; cnt++)
                {
                    if (cnt < intBytes.Length)
                        OutArray[cnt] = intBytes[cnt];
                    else
                        OutArray[cnt] = 0;
                }
            }
            else
            {
                if (Number < intBytes.Length)
                {
                    for (int cnt = 0; cnt < Number; cnt++)
                        OutArray[cnt] = intBytes[cnt];
                }
                else
                {
                    return Encoding.GetEncoding(1252).GetString(intBytes, 0, intBytes.Length);
                }
            }
            return Encoding.GetEncoding(1252).GetString(OutArray, 0, OutArray.Length);
        }


        static CasClass Scale = new CasClass();
        static void Main(string[] args)
        {

            //Scale.ReadSale("192.168.5.137", 20304);
            //Scale.SendAck("192.168.5.102",20304);

            //int RetStatus = Scale.ReadAllPlus("192.168.5.102", 20304);
            //int AllStatus = Scale.ReadAll("192.168.5.102", 20304);
            //int PluCountAvailabelInScale = Scale.AllPlu.Count();
            int PluStatus = Scale.ReadPlu("192.168.5.102", 20304, 1, 1);
            PluStatus = Scale.ReadPlu("192.168.5.102", 20304, 1, 2);

            // int RetStatus = Scale.ReadStore("192.168.5.102", 20304,3);
            //Console.WriteLine("\r\nStore ID = {0}", 1);
            //Console.WriteLine("\r\nStore Name = {0}", Scale.StoreData.Name);
            //Console.WriteLine("\r\nStore Phone = {0}", Scale.StoreData.Phone);
            //Console.WriteLine("\r\nStore Description = {0}", Scale.StoreData.Description);


            //RetStatus = Scale.ReadStore("192.168.5.102", 20304, 2);
            //Console.WriteLine("\r\nStore ID = {0}", 2);
            //Console.WriteLine("\r\nStore Name = {0}", Scale.StoreData.Name);
            //Console.WriteLine("\r\nStore Phone = {0}", Scale.StoreData.Phone);
            //Console.WriteLine("\r\nStore Description = {0}", Scale.StoreData.Description);

            //Scale.StoreData.Code = 1;
            //Scale.StoreData.Description = "فروشگاه میوه های صادراتی";
            //Scale.StoreData.Name = "هایپر میوه سمیعی";
            //Scale.StoreData.Phone = "00989122607039";
            //Scale.WriteStore("192.168.5.102", 20304);


            //Scale.PluData.Name1 = "بستنی میهن";
            //Scale.PluData.PLU_No = 1;
            //Scale.PluData.DepartmentNo = 1;
            //Scale.PluData.PLU_Type = 1;
            //Scale.PluData.Unit_Price = 1200;
            //Scale.PluData.TareValue = 0;
            //Scale.PluData.Tare_No = 0;
            //Scale.PluData.Itemcode = "10100";

            //int Status = Scale.WritePlu("192.168.5.102", 20304);
            //Console.WriteLine("Status = {0}", Status);
            //Console.ReadKey();
        }
    }
}
