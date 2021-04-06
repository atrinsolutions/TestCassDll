using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCassDll.Models
{
    public class Store
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public int Pack_Lenght { get; set; }
        public int Scale_ID { get; set; }
        public string Pack_IP { get; set; }
        public int Pack_Port { get; set; }
        public byte Lock_Info { get; set; }
        public byte Scale_Service_Type { get; set; }
        public int Row { get; set; }
    }
}
