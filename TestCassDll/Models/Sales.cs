using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCassDll.Models
{
    class Sales
    {
        public int Pack_Lenght { get; set; }
        public int Scale_ID { get; set; }
        public string Pack_IP { get; set; }
        public int Pack_Port { get; set; }
        public byte Lock_Info { get; set; }
        public byte Scale_Service_Type { get; set; }
        public int  Row { get; set; }
        public byte PLU_Type { get; set; }
        public byte Department_No { get; set; }
        public int PLU_No { get; set; }
        public int Item_Code { get; set; }
        public int Weight { get; set; }
        public short Qty { get; set; }
        public short Pcs { get; set; }
        public int Unit_Price { get; set; }
        public int Total_Price { get; set; }
        public int Discount_Price { get; set; }
        public int Scale_Transaction_Counter { get; set; }
        public short Ticket_Number { get; set; }
        public short CurrentDate_Year { get; set; }
        public short CurrentDate_Month { get; set; }
        public short CurrentDate_Day { get; set; }
        public short CurrentTime_Hour { get; set; }
        public short CurrentTime_Min { get; set; }
        public short CurrentTime_Second { get; set; }
        public short SaleDate_Year { get; set; }
        public short SaleDate_Month { get; set; }
        public short SaleDate_Day { get; set; }
        public string Barcode { get; set; }
        public string Trace_Code { get; set; }
        public byte Current_Ticket_Sale_Order { get; set; }
        public string Plu_Name { get; set; }
        public bool Negative_Sale { get; set; }
        public bool Return { get; set; }
        public bool Void { get; set; }
        public bool Prepack { get; set; }
        public bool Label { get; set; }
        public bool Override { get; set; }
        public bool Add { get; set; }
        public bool NoVoid { get; set; }
        public bool Normal { get; set; }
        public bool Prepack_Data { get; set; }
        public bool Self_Service { get; set; }
        public bool Plu_Data { get; set; }
        public bool Ticket_Data { get; set; }

    }
}
