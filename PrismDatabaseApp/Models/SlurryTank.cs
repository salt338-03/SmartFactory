using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDatabaseApp.Models
{
    public class SlurryTank
    {
        public string Timestamp { get; set; }
        public double SupplySpeed { get; set; }
        public double RemainingVolume { get; set; }
        public double Temperature { get; set; }
    }
}
