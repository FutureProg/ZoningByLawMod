using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ZoningByLaw.Prefab
{    

    public class ByLawZonePrefab : ZonePrefab
    {

        public ByLawZoneType zoneType;
        public float maxHeight;
        public float minHeight;

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<ByLawZoneFlag>());
        }
    }

    public struct ByLawZoneFlag : IComponentData
    {}

    public enum ByLawZoneType : byte
    {
        Residential = 1,
        Commercial,
        Industrial,
        Office
    }
}
