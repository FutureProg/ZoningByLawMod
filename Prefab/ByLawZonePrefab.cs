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

        public ByLawZoneType zoneType = ByLawZoneType.None;
        public Bounds1 height = new Bounds1(-1, -1);
        public Bounds1 lotSize = new Bounds1(-1, -1);
        public Bounds1 frontage = new Bounds1(-1, -1);
        public Bounds1 parking = new Bounds1(-1, -1);

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
