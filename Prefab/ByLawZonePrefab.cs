using Colossal.Mathematics;
using Colossal.UI.Binding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.UISystems;
using Unity.Entities;
using Unity.Mathematics;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Prefab
{   

    public class ByLawZonePrefab : ZonePrefab, IJsonWritable, IJsonReadable
    {

        public ByLawZoneType zoneType = ByLawZoneType.None;
        public Bounds1 height = new Bounds1(-1, -1);
        public Bounds1 lotSize = new Bounds1(-1, -1);
        public Bounds1 frontage = new Bounds1(-1, -1);
        public Bounds1 parking = new Bounds1(-1, -1);
        public string bylawName;
        public List<ByLawBlockBinding> blocks = new List<ByLawBlockBinding>();

        public bool deleted = false;

        public override void GetPrefabComponents(HashSet<ComponentType> components)
        {
            base.GetPrefabComponents(components);
            components.Add(ComponentType.ReadWrite<ByLawBlockReference>());
            components.Add(ComponentType.ReadWrite<ByLawZoneData>());
        }

        public void Update(ZoningByLawBinding binding)
        {
            if (this.blocks == null)
            {
                this.blocks = new List<ByLawBlockBinding>();
            }
            this.blocks.Clear();      
            this.blocks.AddRange(binding.blocks);       
            this.deleted = binding.deleted;
        }

        public string CreateDescription()
        {
            string re = "";
            if (this.blocks.Count() == 0) {
                return "";
            }
            ByLawBlockBinding block = this.blocks.FirstOrDefault();
            Array.ForEach(block.itemData, item =>
            {                
                Type enumType = BuildingBlockSystem.GetConstraintEnumType(item.byLawItemType);
                var localeDict = GameManager.instance.localizationManager.activeDictionary;
                localeDict.TryGetValue("ZBL.PropertyOperator[" + item.propertyOperator.ToString() + "]", out string operatorString);
                localeDict.TryGetValue("ZBL.ByLawItemType[" + Enum.GetName(typeof(ByLawItemType), item.byLawItemType) + "]", out string itemTypeString);
                re +=  itemTypeString + " " + operatorString + ": ";
                if (enumType != null)
                { 
                    ArrayList valueStrings = new ArrayList();
                    foreach (object enumValue in Enum.GetValues(enumType))
                    {
                        bool isFlag = enumValue is int || enumValue is byte;
                        if((item.valueByteFlag & Convert.ToInt32(enumValue)) != 0)
                        {
                            if (localeDict.TryGetValue($"ZBL.FlagValues[{Enum.GetName(enumType, enumValue)}]", out string valueString)) {
                                valueStrings.Add(valueString);
                            }
                            else
                            {
                                valueStrings.Add(Enum.GetName(enumType, enumValue));
                            }                            
                        }                        
                    }
                    if (valueStrings.Count > 0)
                    {
                        re += valueStrings.ToArray().Aggregate((a, b) => a + ", " + b);
                    }                    
                }  
                else if (item.valueNumber != 0)
                {
                    re += ": " + item.valueNumber;
                }
                else if (item.valueBounds1.min != -1 || item.valueBounds1.max != -1)
                {
                    re += " min: " + (item.valueBounds1.min == -1? "None": item.valueBounds1.min) + " max: " + (item.valueBounds1.max == -1? "None": item.valueBounds1.max);
                }
                re += "\n";
            });

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
