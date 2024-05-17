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

    public class ByLawZonePrefab : ZonePrefab
    {

        public ByLawZoneType zoneType;
        public Bounds1 height;
        public Bounds1 lotSize;
        public Bounds1 frontage;
        public Bounds1 parking;

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<ByLawZoneData>());
        }
    }

    public struct ByLawZoneData : IComponentData, IJsonWritable, IJsonReadable
    {

        public ByLawZoneType zoneType;
        public Bounds1 height;
        public Bounds1 lotSize;
        public Bounds1 frontage;
        public Bounds1 parking;

        public bool deleted; // deleted bylaws shouldn't show up in the UI, and won't be serialized

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
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

        public void Write(IJsonWriter writer)
        {
            if (deleted)
            {
                return;
            }
            writer.TypeBegin(GetType().FullName);
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
