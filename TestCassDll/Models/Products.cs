using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCassDll
{
    public class Product
    {
        public int DepartmentNo { get; set; }
        public int PLU_No { get; set; }
        public int PLU_Type { get; set; }
        public string Itemcode { get; set; }
        public string Name1 { get; set; }
        private string Name2 { get; set; }
        private string Name3 { get; set; }
        public int Group_No { get; set; }
        public int Label_No { get; set; }
        public int Ax_Label_No { get; set; }
        public int Origin_No { get; set; }
        public string Direct_Ingradiant { get; set; }
        public string Direct_Ingradiant2 { get; set; }
        public int Unit_Weight { get; set; }
        public string Fixed_Weight { get; set; }
        public string Image { get; set; }
        public int Unit_Price { get; set; }
        public string Pieces { get; set; }
        public string Sell_By_Date { get; set; }
        public string Sell_By_Time { get; set; }
        public int Ingradient_No { get; set; }
        public int TareValue { get; set; }
        public int Tare_No { get; set; }
        public string Packed_Date { get; set; }
        public int Special_Price { get; set; }
        public int Barcode_No { get; set; }
        public int Barcode2_No { get; set; }
        public bool Use_Fixed_Price_Type { get; set; }
        public int TaxCode { get; set; }
        public int SaleMessage_No { get; set; }
        public int PackLenght { get; set; }
        public int ScaleID { get; set; }
        public string PackIP { get; set; }
        public int PackPort { get; set; }
        public bool LockInfo { get; set; }
        public byte ScaleServiceType { get; set; }
        public int TableRow { get; set; }
    }
}
