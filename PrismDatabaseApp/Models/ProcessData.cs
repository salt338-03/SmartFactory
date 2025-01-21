using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrismDatabaseApp.Models
{
    public class ProcessData
    {
        public SlurryTank SlurryTank { get; set; }
        public CoatingProcess CoatingProcess { get; set; }
        public DryingProcess DryingProcess { get; set; }
    }
}
