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
            int i = 0;
            
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

            //byte t1 = 0xaa;
            //string Ans= Scale.MakeStr1252 (t1);
            //byte[] EncodeArray = Encoding.GetEncoding(1252).GetBytes(Ans);

            //short t2 = 0xaa;
            //string Ans1 = Scale.MakeStr1252(t2);
            //byte[] EncodeArray1 = Encoding.GetEncoding(1252).GetBytes(Ans1);

            //int t3 = 0x000002ee;
            //string Ans2 = Scale.MakeStr1252(t3);
            //byte[] EncodeArray2 = Encoding.GetEncoding(1252).GetBytes(Ans2);

            ////            Array.Reverse(intBytes);
            ////          byte[] result = intBytes;


            ////    int RetStatus = Scale.ReadAllPlus("192.168.5.102", 20304, false, 0);
            //int AllStatus = Scale.ReadAll("192.168.5.102", 20304);
            //int PluCountAvailabelInScale = Scale.AllPlu.Count();
            //int PluStatus = Scale.ReadPlu("192.168.5.102", 20304,2);

            Scale.PluData.Name1 = "سلام جیگر";
            Scale.PluData.PLU_No = 2;
            Scale.PluData.Unit_Price = 4780;
            Scale.PluData.TareValue = 0;
            Scale.PluData.Tare_No = 0;
            Scale.PluData.Itemcode = "1234";

            Scale.WritePlu("192.168.5.102", 20304);
        }
    }
}
