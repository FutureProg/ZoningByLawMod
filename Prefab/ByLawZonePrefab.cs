using Colossal.Mathematics;
using Colossal.UI.Binding;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace Trejak.ZoningByLaw.Prefab
{

    public class ByLawZonePrefab : ZonePrefab, IJsonWritable, IJsonReadable
    {

        public ByLawZoneType zoneType = ByLawZoneType.None;
        public Bounds1 height = new Bounds1(-1, -1);
        public Bounds1 lotSize = new Bounds1(-1, -1);
        public Bounds1 frontage = new Bounds1(-1, -1);
        public Bounds1 parking = new Bounds1(-1, -1);
        public bool deleted = false;

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<ByLawZoneData>());
        }

        public string CreateDescription()
        {
            string re = "";

            re += "Permitted Uses: ";
            var usesValArr = Enum.GetValues(typeof(ByLawZoneType));
            for (int i = 0; i < usesValArr.Length; i++)
            {
                ByLawZoneType val = (ByLawZoneType)usesValArr.GetValue(i);
                if ((val & zoneType) != 0)
                {
                    re += Enum.GetName(typeof(ByLawZoneType), val) + ", ";
                }
            }
            if (re.EndsWith(", "))
            {
                re = re.Substring(0, re.Length - 2);
            }
            else
            {
                re += "None";
            }
            re += "\n";
            re += $"Height: Min={(height.min >= 0 ? height.min : "None")}, Max={(height.max >= 0 ? height.max : "None")}\n";
            re += $"Lot Size: Min={(lotSize.min >= 0 ? lotSize.min : "None")}, Max={(lotSize.max >= 0 ? lotSize.max : "None")}\n";
            re += $"Frontage: Min={(frontage.min >= 0 ? frontage.min : "None")}, Max={(frontage.max >= 0 ? frontage.max : "None")}\n";
            return re;
        }

        public void Write(IJsonWriter writer)
        {
            if (deleted)
            {
                return;
            }
            writer.TypeBegin(nameof(ByLawZonePrefab));
            writer.PropertyName("bylawName");
            writer.Write((string)name);
            writer.PropertyName("zoneType");
            writer.Write((int)zoneType);
            writer.PropertyName("height");
            writer.Write(height);
            writer.PropertyName("lotSize");
            writer.Write(lotSize);
            writer.PropertyName("frontage");
            writer.Write(frontage);
            writer.PropertyName("parking");
            writer.Write(parking);
            writer.TypeEnd();
        }

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty("bylawName");
            reader.Read(out string tname);
            this.name = tname;
            reader.ReadProperty("zoneType");
            reader.Read(out int zoneTypeInt);
            this.zoneType = (ByLawZoneType)zoneTypeInt;
            reader.ReadProperty("height");
            reader.Read(out this.height);
            reader.ReadProperty("lotSize");
            reader.Read(out lotSize);
            reader.ReadProperty("frontage");
            reader.Read(out frontage);
            reader.ReadProperty("parking");
            reader.Read(out this.parking);
            reader.ReadMapEnd();
        }
    }

    public enum ByLawZoneType : byte
    {
        None = 0,
        Residential = 1,
        Commercial = 2,
        Industrial = 4,
        Office = 8
    }
}
