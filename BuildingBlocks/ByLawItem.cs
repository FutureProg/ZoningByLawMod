using Colossal.Mathematics;
using Unity.Collections;
using Unity.Entities;

namespace Trejak.ZoningByLaw.BuildingBlocks
{
    public struct ByLawItem : IBufferElementData
    {

        public ByLawItemType byLawItemType; // The type 
        public ByLawConstraintType constraintType; // The one constraint chosen by the player
        public ByLawItemCategory itemCategory; // The one category chosen by the player
        public ByLawPropertyOperator propertyOperator;


        public Bounds1 valueBounds1;
        public int valueByteFlag;
        public int valueNumber;
        public NativeArray<int> valueNumberArray;
    }

    public enum ByLawItemType : int
    {
        None = 0,
        Uses,
        Height,
        LotWidth,
        LotSize,
        LotDepth,
        Parking,
        FrontSetback,
        LeftSetback,
        RightSetback,
        RearSetback,
        AirPollutionLevel,
        GroundPollutionLevel,
        NoisePollutionLevel,
        AssetPack,
        Density
    }

    public enum ByLawConstraintType : int
    {
        None = 0,        
        Length = 1,
        Count = 2,
        MultiSelect = 3,
        SingleSelect = 4
    }

    public enum ByLawItemCategory : int
    {
        None = 0,
        Building = 1,
        Lot = 2,
        Pollution = 3
    }

    public enum ByLawPropertyOperator : int
    {
        None = 0,
        Is = 1,
        IsNot = 2,
        AtLeastOne = 3,
        OnlyOneOf = 4,
        AtMost = 5,
        AtLeast = 6
    }
}
