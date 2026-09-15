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
        AssetPack, // Deprecated: superseded by AssetStyle (see issue #25). Kept only so old saves still parse; ByLawRecord migrates it to AssetStyle on load.
        Density,
        Theme, // Deprecated: superseded by AssetStyle (see issue #25). Kept only so old saves still parse; ByLawRecord migrates it to AssetStyle on load.
        AssetStyle // Combines Asset Pack and Asset Theme selection into one constraint: matches the union of selected packs/themes ("at least one"), or none of them ("is not").
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
