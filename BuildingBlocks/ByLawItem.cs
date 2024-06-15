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
        public ByLawPropertyOperator propertyOperator;


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
            this.constraintType = (ByLawConstraintType)(byte)enumTemp;
            reader.ReadProperty(nameof(itemCategory));
            reader.Read(out enumTemp);
            this.itemCategory = (ByLawItemCategory)(byte)enumTemp;
            reader.ReadProperty(nameof(propertyOperator));
            reader.Read(out enumTemp);
            this.propertyOperator = (ByLawPropertyOperator)enumTemp;

            Mod.log.Info("Read " + nameof(valueBounds1));
            reader.ReadProperty(nameof(valueBounds1));
            reader.Read(out this.valueBounds1);
            int tempInt;
            Mod.log.Info("Read " + nameof(valueByteFlag));
            reader.ReadProperty(nameof(valueByteFlag));                        
            reader.Read(out tempInt);
            this.valueByteFlag = (byte)tempInt;
            Mod.log.Info("Read " + nameof(valueNumber));
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
            writer.PropertyName(nameof(propertyOperator));
            writer.Write((int)propertyOperator);

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
        Length = 1,
        Count = 2,
        MultiSelect = 3,
        SingleSelect = 4
    }

    public enum ByLawItemCategory : byte
    {
        None = 0,
        Building = 1,
        Lot = 2,
        Pollution = 3
    }

    public enum ByLawPropertyOperator : uint
    {
        None = 0,
        Is = 1,
        IsNot = 2,
        AtLeastOne = 3,
        OnlyOneOf = 4
    }
}
