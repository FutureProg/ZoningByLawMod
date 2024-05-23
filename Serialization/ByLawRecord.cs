using Colossal.UI.Binding;
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
    public class ByLawRecord : IJsonWritable, IJsonReadable
    {

        public string bylawName;
        public string bylawDesc;
        public Color zoneColor;
        public Color edgeColor;
        public ByLawZoneData bylawZoneData;

        public ByLawRecord()
        {

        }

        public ByLawRecord(string name, string description, Color zoneColor, Color edgeColor, ByLawZoneData data)
        {
            this.bylawZoneData = data;
            this.bylawName = name;
            this.zoneColor = zoneColor;
            this.edgeColor = edgeColor;
            this.bylawDesc = description;
        }

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty("name");
            reader.Read(out this.bylawName);
            reader.ReadProperty("description");
            reader.Read(out this.bylawDesc);
            reader.ReadProperty("zoneColor");
            reader.Read(out this.zoneColor);
            reader.ReadProperty("edgeColor");
            reader.Read(out this.edgeColor);

            reader.ReadProperty("bylawData");            
            ByLawZoneData data = new();
            data.Read(reader);
            reader.ReadMapEnd();
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(nameof(ByLawRecord));
            writer.PropertyName("name");
            writer.Write(bylawName);
            writer.PropertyName("description");
            writer.Write(bylawDesc);
            writer.PropertyName("zoneColor");
            writer.Write(this.zoneColor);
            writer.PropertyName("edgeColor");
            writer.Write(this.edgeColor);

            writer.PropertyName("bylawData");
            writer.Write(bylawZoneData);            
            writer.TypeEnd();
        }
    }
}
