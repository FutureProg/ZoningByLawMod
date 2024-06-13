using Colossal.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;

namespace Trejak.ZoningByLaw.BuildingBlocks
{
    public struct ByLawItem
    {

        public ByLawItemType byLawItemType; // The type 
        public ByLawConstraintType constraintType; // The one constraint chosen by the player
        public ByLawItemCategory itemCategory; // The one category chosen by the player

        public Bounds1 valueBounds1;
        public byte valueByteFlag;
    }

    public enum ByLawItemType : uint
    {
        None = 0,
        Uses,
        Height,
        LotWidth,
        LotSize,
        Parking,
        FrontSetback,
        LeftSetback,
        RightSetback,
        RearSetback,
        AirPollutionLevel,
        GroundPollutionLevel,
        NoisePollutionLevel
    }

    public enum ByLawConstraintType : byte
    {
        None = 0,        
        Length,
        Count,
        MultiSelect,
        SingleSelect
    }

    public enum ByLawItemCategory : byte
    {
        None = 0,
        Building = 1,
        Lot,
        Pollution
    }
}
