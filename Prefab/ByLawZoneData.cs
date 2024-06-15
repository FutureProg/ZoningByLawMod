using Colossal.Mathematics;
using Colossal.UI.Binding;
using System;
using Trejak.ZoningByLaw.BuildingBlocks;
using Unity.Collections;
using Unity.Entities;

namespace Trejak.ZoningByLaw.Prefab
{
    public struct ByLawZoneData : IComponentData//, IJsonWritable, IJsonReadable
    {
        [Obsolete]
        public ByLawZoneType zoneType;
        [Obsolete]
        public Bounds1 height;
        [Obsolete]
        public Bounds1 lotSize;
        [Obsolete]
        public Bounds1 frontage;
        [Obsolete]
        public Bounds1 parking;
        


        public bool deleted; // deleted bylaws shouldn't show up in the UI, and won't be serialized

        //public void Read(IJsonReader reader)
        //{
        //    reader.ReadMapBegin();
        //    reader.ReadProperty("zoneType");
        //    reader.Read(out int zoneTypeInt);
        //    this.zoneType = (ByLawZoneType)zoneTypeInt;
        //    reader.ReadProperty("height");
        //    reader.Read(out this.height);
        //    reader.ReadProperty("lotSize");
        //    reader.Read(out lotSize);
        //    reader.ReadProperty("frontage");
        //    reader.Read(out frontage);
        //    reader.ReadProperty("parking");
        //    reader.Read(out this.parking);           
        //    reader.ReadMapEnd();
        //}

        //public void Write(IJsonWriter writer)
        //{
        //    if (deleted)
        //    {
        //        return;
        //    }
        //    writer.TypeBegin(GetType().FullName);
        //    writer.PropertyName("zoneType");
        //    writer.Write((int)zoneType);
        //    writer.PropertyName("height");
        //    writer.Write(height);
        //    writer.PropertyName("lotSize");
        //    writer.Write(lotSize);
        //    writer.PropertyName("frontage");
        //    writer.Write(frontage);
        //    writer.PropertyName("parking");
        //    writer.Write(parking);
        //    writer.TypeEnd();
        //}

        //public string CreateDescription()
        //{
        //    string re = "";

        //    re += "Permitted Uses: ";
        //    var usesValArr = Enum.GetValues(typeof(ByLawZoneType));
        //    for (int i = 0; i < usesValArr.Length; i++)
        //    {
        //        ByLawZoneType val = (ByLawZoneType)usesValArr.GetValue(i);
        //        if ((val & zoneType) != 0)
        //        {
        //            re += Enum.GetName(typeof(ByLawZoneType), val) + ", ";
        //        }
        //    }
        //    if (re.EndsWith(", "))
        //    {
        //        re = re.Substring(0, re.Length - 2);
        //    }
        //    else
        //    {
        //        re += "None";
        //    }
        //    re += "\n";
        //    re += $"Height: Min={(height.min >= 0 ? height.min : "None")}, Max={(height.max >= 0 ? height.max : "None")}\n";
        //    re += $"Lot Size: Min={(lotSize.min >= 0 ? lotSize.min : "None")}, Max={(lotSize.max >= 0 ? lotSize.max : "None")}\n";
        //    re += $"Frontage: Min={(frontage.min >= 0 ? frontage.min : "None")}, Max={(frontage.max >= 0 ? frontage.max : "None")}\n";
        //    return re;
        //}
    }
}
