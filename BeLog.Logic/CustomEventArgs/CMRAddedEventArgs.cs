using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeLog.Logic.CustomEventArgs
{
    public class CMRAddedEventArgs:EventArgs
    {
        public int CMRId { get; set; }
        public CMRAddedEventArgs(int cmrId)
        {
            CMRId = cmrId;
        }
    }
}
