using Colossal.Mathematics;
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
        public Bounds2 height;
        public Bounds2 lotSize;
        public Bounds2 frontage;
        public Bounds2 parking;

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<ByLawZoneData>());
        }
    }

    public struct ByLawZoneData : IComponentData
    {

        public ByLawZoneType zoneType;
        public Bounds2 height;
        public Bounds2 lotSize;
        public Bounds2 frontage;
        public Bounds2 parking;

    }

    public enum ByLawZoneType : byte
    {
        None = 0,
        Residential = 1,
        Commercial,
        Industrial,
        Office
    }
}
