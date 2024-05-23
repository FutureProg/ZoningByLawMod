using Colossal.UI.Binding;
using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using UnityEngine;

namespace Trejak.ZoningByLaw.Serialization
{

    /// <summary>
    /// Holds a list of all the bylaws and writes them to disk
    /// </summary>
    public class ByLawRecord
    {

        public string bylawName;
        public string idName;
        public string bylawDesc;
        public Color zoneColor;
        public Color edgeColor;
        public ByLawZoneData bylawZoneData;        

        public ByLawRecord()
        {

        }

        public ByLawRecord(string name, string description, Color zoneColor, Color edgeColor, ByLawZoneData data, PrefabID prefabID)
        {
            this.bylawZoneData = data;
            this.bylawName = name;
            this.zoneColor = zoneColor;
            this.edgeColor = edgeColor;
            this.bylawDesc = description;
            this.idName = prefabID.GetName();
        }
    }
}
