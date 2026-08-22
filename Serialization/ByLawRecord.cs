using Colossal.UI.Binding;
using Game.Prefabs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.UISystems;
using Unity.Collections;
using UnityEngine;
using ZoningByLaw.BuildingBlocks;

namespace Trejak.ZoningByLaw.Serialization
{

    /// <summary>
    /// Holds a list of all the bylaws and writes them to disk
    /// </summary>
    public class ByLawRecord
    {
        public static FormatVersion CURRENT_SERIALIZATION_VERSION = FormatVersion.V_0_1_BASE;
        public FormatVersion serialVersion = CURRENT_SERIALIZATION_VERSION;
        public string bylawName;
        public string idName;
        public string bylawDesc;
        
        [JsonIgnore]
        public Color zoneColor;
        
        [JsonIgnore]
        public Color edgeColor;
        
        [JsonIgnore]
        public ByLawZoneData bylawZoneData;
        
        [JsonIgnore]
        public ZoningByLawBinding zoningByLawBinding;

        // Serializable properties for Color - uses RGBA string format
        [JsonProperty("zoneColor")]
        public string ZoneColorSerialized
        {
            get => ColorToRGBAString(zoneColor);
            set => zoneColor = RGBAStringToColor(value);
        }

        [JsonProperty("edgeColor")]
        public string EdgeColorSerialized
        {
            get => ColorToRGBAString(edgeColor);
            set => edgeColor = RGBAStringToColor(value);
        }

        // Serializable property for ByLawZoneData (backward compatibility - obsolete fields will be ignored on write)
        [JsonProperty("bylawZoneData")]
        public SerializableByLawZoneData ByLawZoneDataSerialized
        {
            get => new SerializableByLawZoneData(bylawZoneData);
            set => bylawZoneData = value.ToByLawZoneData();
        }

        // Serializable property for ZoningByLawBinding
        [JsonProperty("zoningByLawBinding")]
        public SerializableZoningByLawBinding ZoningByLawBindingSerialized
        {
            get => new SerializableZoningByLawBinding(zoningByLawBinding);
            set => zoningByLawBinding = value.ToZoningByLawBinding();
        }

        public ByLawRecord()
        {

        }

        [Obsolete]
        public ByLawRecord(string name, string description, Color zoneColor, Color edgeColor, ByLawZoneData data, PrefabID prefabID)
        {
            this.bylawZoneData = data;
            this.bylawName = name;
            this.zoneColor = zoneColor;
            this.edgeColor = edgeColor;
            this.bylawDesc = description;
            this.idName = prefabID.GetName();
        }

        public ByLawRecord(string name, string description, Color zoneColor, Color edgeColor, ZoningByLawBinding data, PrefabID prefabID)
        {
            this.zoningByLawBinding = data;
            this.bylawName = name;
            this.zoneColor = zoneColor;
            this.edgeColor = edgeColor;
            this.bylawDesc = description;
            this.idName = prefabID.GetName();
        }

        public enum FormatVersion
        {
            V_0_1_BASE = 0
        }

        private static string ColorToRGBAString(Color color)
        {
            return $"RGBA({color.r.ToString(CultureInfo.InvariantCulture)}, {color.g.ToString(CultureInfo.InvariantCulture)}, {color.b.ToString(CultureInfo.InvariantCulture)}, {color.a.ToString(CultureInfo.InvariantCulture)})";
        }

        private static Color RGBAStringToColor(string rgba)
        {
            if (string.IsNullOrEmpty(rgba))
                return Color.white;

            // Parse "RGBA(r, g, b, a)"
            rgba = rgba.Replace("RGBA(", "").Replace(")", "").Trim();
            string[] components = rgba.Split(',');
            
            if (components.Length != 4)
                return Color.white;

            float r = float.Parse(components[0].Trim(), CultureInfo.InvariantCulture);
            float g = float.Parse(components[1].Trim(), CultureInfo.InvariantCulture);
            float b = float.Parse(components[2].Trim(), CultureInfo.InvariantCulture);
            float a = float.Parse(components[3].Trim(), CultureInfo.InvariantCulture);

            return new Color(r, g, b, a);
        }
    }

    /// <summary>
    /// Serializable representation of ByLawZoneData for JSON (backward compatibility)
    /// Only non-obsolete fields are serialized on write
    /// </summary>
    public struct SerializableByLawZoneData
    {
        // Obsolete fields - only for reading old files
        [JsonProperty("zoneType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string zoneType;
        
        [JsonProperty("height", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SerializableBounds1? height;
        
        [JsonProperty("lotSize", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SerializableBounds1? lotSize;
        
        [JsonProperty("frontage", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SerializableBounds1? frontage;
        
        [JsonProperty("parking", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SerializableBounds1? parking;

        // Non-obsolete fields - always serialized
        public int elligibleBuildings;
        public bool deleted;

        public SerializableByLawZoneData(ByLawZoneData data)
        {
            // Don't serialize obsolete fields when writing
            zoneType = null;
            height = null;
            lotSize = null;
            frontage = null;
            parking = null;
            
            // Serialize non-obsolete fields
            elligibleBuildings = data.elligibleBuildings;
            deleted = data.deleted;
        }

        public ByLawZoneData ToByLawZoneData()
        {
            var data = new ByLawZoneData
            {
                elligibleBuildings = elligibleBuildings,
                deleted = deleted
            };

            // Handle obsolete fields if present in old files (for backward compatibility)
#pragma warning disable CS0618 // Type or member is obsolete
            if (!string.IsNullOrEmpty(zoneType) && Enum.TryParse<ByLawZoneType>(zoneType, out var zt))
            {
                data.zoneType = zt;
            }
            
            if (height.HasValue)
            {
                data.height = height.Value.ToBounds1();
            }
            
            if (lotSize.HasValue)
            {
                data.lotSize = lotSize.Value.ToBounds1();
            }
            
            if (frontage.HasValue)
            {
                data.frontage = frontage.Value.ToBounds1();
            }
            
            if (parking.HasValue)
            {
                data.parking = parking.Value.ToBounds1();
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return data;
        }
    }

    /// <summary>
    /// Serializable representation of ZoningByLawBinding for JSON
    /// </summary>
    public struct SerializableZoningByLawBinding
    {
        public SerializableByLawBlockBinding[] blocks;
        public bool deleted;

        public SerializableZoningByLawBinding(ZoningByLawBinding binding)
        {
            deleted = binding.deleted;
            blocks = binding.blocks?.Select(b => new SerializableByLawBlockBinding(b)).ToArray();
        }

        public ZoningByLawBinding ToZoningByLawBinding()
        {
            return new ZoningByLawBinding
            {
                deleted = deleted,
                blocks = blocks?.Select(b => b.ToByLawBlockBinding()).ToArray() ?? new ByLawBlockBinding[0]
            };
        }
    }

    /// <summary>
    /// Serializable representation of ByLawBlockBinding for JSON
    /// </summary>
    public struct SerializableByLawBlockBinding
    {
        public SerializableByLawBlock blockData;
        public SerializableByLawItem[] itemData;

        public SerializableByLawBlockBinding(ByLawBlockBinding binding)
        {
            blockData = new SerializableByLawBlock(binding.blockData);
            itemData = binding.itemData?.Select(i => new SerializableByLawItem(i)).ToArray();
        }

        public ByLawBlockBinding ToByLawBlockBinding()
        {
            return new ByLawBlockBinding
            {
                blockData = blockData.ToByLawBlock(),
                itemData = itemData?.Select(i => i.ToByLawItem()).ToArray() ?? new BuildingBlocks.ByLawItem[0]
            };
        }
    }

    /// <summary>
    /// Serializable representation of ByLawBlock for JSON
    /// </summary>
    public struct SerializableByLawBlock
    {
        [JsonProperty("blockType")]
        public string blockType;
        
        [JsonProperty("logicOperation")]
        public string logicOperation;

        public SerializableByLawBlock(BuildingBlocks.ByLawBlock block)
        {
            blockType = block.blockType.ToString();
            logicOperation = block.logicOperation.ToString();
        }

        public BuildingBlocks.ByLawBlock ToByLawBlock()
        {
            return new BuildingBlocks.ByLawBlock
            {
                blockType = Enum.TryParse<BuildingBlocks.BlockType>(blockType, out var bt) ? bt : BuildingBlocks.BlockType.Instruction,
                logicOperation = Enum.TryParse<BuildingBlocks.LogicOperation>(logicOperation, out var lo) ? lo : BuildingBlocks.LogicOperation.None
            };
        }
    }

    /// <summary>
    /// Serializable representation of ByLawItem for JSON
    /// </summary>
    public struct SerializableByLawItem
    {
        [JsonProperty("byLawItemType")]
        public string byLawItemType;
        
        [JsonProperty("constraintType")]
        public string constraintType;
        
        [JsonProperty("itemCategory")]
        public string itemCategory;
        
        [JsonProperty("propertyOperator")]
        public string propertyOperator;
        
        public SerializableBounds1 valueBounds1;
        public int valueByteFlag;
        public int valueNumber;
        public int[] valueNumberArray;

        // Asset pack membership is persisted by stable string name, not by the hashes used at runtime
        // (string.GetHashCode() isn't guaranteed stable across process restarts). Hashes are recomputed
        // from these names on load, via the same AssetPackHashUtils.NameToHash used by IndexBuildingsSystem,
        // so a saved by-law always matches the live index within a session. Records saved before this field
        // existed have no assetPackNames and are treated as an empty selection (their old raw-hash
        // valueNumberArray is intentionally ignored for AssetPack items, since it was never valid to begin with).
        [JsonProperty("assetPackNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] assetPackNames;

        // Theme membership is persisted by stable string name for the same reason as assetPackNames
        // above: string.GetHashCode() isn't guaranteed stable across process restarts. Resolved back to
        // hashes on load via ThemeHashUtils.NameToHash.
        [JsonProperty("themeNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] themeNames;

        public SerializableByLawItem(BuildingBlocks.ByLawItem item)
        {
            byLawItemType = item.byLawItemType.ToString();
            constraintType = item.constraintType.ToString();
            itemCategory = item.itemCategory.ToString();
            propertyOperator = item.propertyOperator.ToString();
            valueBounds1 = new SerializableBounds1(item.valueBounds1);
            valueByteFlag = item.valueByteFlag;
            valueNumber = item.valueNumber;
            valueNumberArray = null;
            assetPackNames = null;
            themeNames = null;

            var indexBuildingsSystem = Unity.Entities.World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<IndexBuildingsSystem>();
            if (item.byLawItemType == BuildingBlocks.ByLawItemType.AssetPack)
            {
                assetPackNames = ResolveDynamicMultiSelectNames(item, "AssetPack", hash => indexBuildingsSystem?.GetAssetPackByHash(hash)?.name);
            }
            else if (item.byLawItemType == BuildingBlocks.ByLawItemType.Theme)
            {
                themeNames = ResolveDynamicMultiSelectNames(item, "Theme", hash => indexBuildingsSystem?.GetThemeByHash(hash)?.name);
            }
            else
            {
                valueNumberArray = item.valueNumberArray.IsCreated ? item.valueNumberArray.ToArray() : null;
            }
        }

        // Shared by every dynamic, name-based multi-select item type (AssetPack, Theme, ...): resolves the
        // runtime hash selection back to stable names for persistence, since string.GetHashCode() isn't
        // guaranteed stable across process restarts. Every hash is expected to resolve, since it can only
        // have been selected from an option list the same index already produced - if some don't, the index
        // isn't ready (e.g. saved before IndexBuildingsSystem finished its first pass, or during world
        // teardown), which is surfaced here instead of silently dropping the user's selection on disk.
        private static string[] ResolveDynamicMultiSelectNames(BuildingBlocks.ByLawItem item, string typeLabel, Func<int, string> hashToName)
        {
            if (!item.valueNumberArray.IsCreated || item.valueNumberArray.Length == 0)
            {
                return null;
            }
            var names = item.valueNumberArray.Select(hashToName).Where(n => n != null).ToArray();
            if (names.Length < item.valueNumberArray.Length)
            {
                Mod.log.Warn(
                    $"{typeLabel} by-law item: could not resolve {item.valueNumberArray.Length - names.Length} " +
                    $"of {item.valueNumberArray.Length} selected hash(es) to a name while saving " +
                    "(IndexBuildingsSystem not ready?); those selections will not be persisted this save.");
            }
            return names;
        }

        public BuildingBlocks.ByLawItem ToByLawItem()
        {
            var parsedItemType = Enum.TryParse<BuildingBlocks.ByLawItemType>(byLawItemType, out var bit) ? bit : BuildingBlocks.ByLawItemType.None;
            int[] numberArray = parsedItemType == BuildingBlocks.ByLawItemType.AssetPack
                ? (assetPackNames ?? new string[0]).Select(AssetPackHashUtils.NameToHash).ToArray()
                : parsedItemType == BuildingBlocks.ByLawItemType.Theme
                    ? (themeNames ?? new string[0]).Select(ThemeHashUtils.NameToHash).ToArray()
                    : (valueNumberArray ?? new int[0]);
            return new BuildingBlocks.ByLawItem
            {
                byLawItemType = parsedItemType,
                // Recomputed from itemType rather than trusted from the serialized value: constraintType is
                // purely a function of byLawItemType (see BuildingBlockSystem.GetConstraintTypes), never a
                // user choice, so this also self-heals saves written while a type's mapping was wrong
                // (e.g. Parking was misclassified as Length instead of Count, which made every Parking
                // by-law item silently fail to match any building).
                constraintType = BuildingBlockSystem.GetConstraintTypes(parsedItemType),
                itemCategory = Enum.TryParse<BuildingBlocks.ByLawItemCategory>(itemCategory, out var ic) ? ic : BuildingBlocks.ByLawItemCategory.None,
                propertyOperator = Enum.TryParse<BuildingBlocks.ByLawPropertyOperator>(propertyOperator, out var po) ? po : BuildingBlocks.ByLawPropertyOperator.None,
                valueBounds1 = valueBounds1.ToBounds1(),
                valueByteFlag = valueByteFlag,
                valueNumber = valueNumber,
                valueNumberArray = new NativeArray<int>(numberArray, Allocator.Persistent)
            };
        }
    }

    /// <summary>
    /// Serializable representation of Bounds1 for JSON
    /// </summary>
    public struct SerializableBounds1
    {
        public float min;
        public float max;

        public SerializableBounds1(Colossal.Mathematics.Bounds1 bounds)
        {
            min = bounds.min;
            max = bounds.max;
        }

        public Colossal.Mathematics.Bounds1 ToBounds1()
        {
            return new Colossal.Mathematics.Bounds1(min, max);
        }
    }
}
