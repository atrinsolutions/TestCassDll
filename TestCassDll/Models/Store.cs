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
        public int PackLenght { get; set; }
        public int ScaleID { get; set; }
        public string PackIP { get; set; }
        public int PackPort { get; set; }
        public bool LockInfo { get; set; }
        public byte ScaleServiceType { get; set; }
        public int TableRow { get; set; }
    }
}
