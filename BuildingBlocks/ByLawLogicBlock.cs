using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Trejak.ZoningByLaw.BuildingBlocks
{
    public enum LogicType : uint
    {
        NONE = 0,
        AND,
        OR,
        THEN
    }

    public struct ByLawLogicBlockData : IByLawBlockData
    {
        public LogicType logicType;        
    }

    
}
