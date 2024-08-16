using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trejak.ZoningByLaw.Prefab
{
    public struct PollutionThresholdData
    {
        public int low;
        public int medium;
        public int high;
    }

    public struct PollutionThresholdsSet
    {
        public PollutionThresholdData air;
        public PollutionThresholdData ground;
        public PollutionThresholdData noise;
    }
}
