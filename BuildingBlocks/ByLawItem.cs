using Colossal.Mathematics;
using Colossal.UI.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Entities;

namespace Trejak.ZoningByLaw.BuildingBlocks
{
    public struct ByLawItem : IBufferElementData, IJsonReadable, IJsonWritable
    {

        public ByLawItemType byLawItemType; // The type 
        public ByLawConstraintType constraintType; // The one constraint chosen by the player
        public ByLawItemCategory itemCategory; // The one category chosen by the player

        public Bounds1 valueBounds1;
        public byte valueByteFlag;
        public int valueNumber;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(byLawItemType));
            reader.Read(out int enumTemp);
            this.byLawItemType = (ByLawItemType)enumTemp;
            reader.ReadProperty(nameof(constraintType));
            reader.Read(out enumTemp);
            this.constraintType = (ByLawConstraintType)enumTemp;
            reader.ReadProperty(nameof(itemCategory));
            reader.Read(out enumTemp);
            this.itemCategory = (ByLawItemCategory)enumTemp;

            reader.ReadProperty(nameof(valueBounds1));
            reader.Read(out this.valueBounds1);
            int tempInt;
            reader.ReadProperty(nameof(valueByteFlag));                        
            reader.Read(out tempInt);
            this.valueByteFlag = (byte)tempInt;
            reader.ReadProperty(nameof(valueNumber));
            reader.Read(out this.valueNumber);
            reader.ReadMapEnd();
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(GetType().FullName);
            writer.PropertyName(nameof(byLawItemType));
            writer.Write((int) this.byLawItemType);
            writer.PropertyName(nameof(constraintType));
            writer.Write((int)constraintType);
            writer.PropertyName(nameof(byLawItemType));
            writer.Write((int)byLawItemType);

            writer.PropertyName(nameof(valueBounds1));
            writer.Write(valueBounds1);
            writer.PropertyName(nameof(valueByteFlag));
            writer.Write(valueByteFlag);
            writer.PropertyName(nameof(valueNumber));
            writer.Write(valueNumber);
            writer.TypeEnd();
        }
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
