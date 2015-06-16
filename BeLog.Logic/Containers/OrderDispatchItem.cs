using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeLog.Logic.Containers
{
    public class OrderDispatchItem
    {
        public int OrderId { get; set; }
        public string DispatchTime { get; set; }
        public string DispatchPlace { get; set; }
        public string DispatchContact { get; set; }
        public string DispatchType { get; set; }
    }
}
