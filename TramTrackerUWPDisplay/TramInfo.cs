using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TramTrackerUWPDisplay
{
    public class TramInfo
    {
        public TramInfo(String routeNumber, String destination, DateTime arrivalTime)
        {
            RouteNumber = routeNumber;
            Destination = destination;
            ArrivalTime = arrivalTime;
        }
        public DateTime ArrivalTime { get; }
        public String RouteNumber { get;  }
        public String Destination { get;  }

    }
}
