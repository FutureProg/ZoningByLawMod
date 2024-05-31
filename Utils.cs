using Game.SceneFlow;
using System.IO;
using Colossal.PSI.Environment;
using Trejak.ZoningByLaw.Prefab;
using Colossal.Json;
using System.Collections.Generic;
using System;
using Unity.Entities;
using Game.Prefabs;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using Trejak.ZoningByLaw.Serialization;
using Game.UI.InGame;

namespace Trejak.ZoningByLaw
{
    public static class Utils
    {

        private static PrefabBase _basePrefab;
        private static PrefabSystem _prefabSystem;
        private static UIAssetCategoryPrefab _assetCategory;
        private static bool _initialized;

        public static string ContentFolder { get; }

        static Utils()
        {
            ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Trejak.ZoningByLaw");
            Directory.CreateDirectory(ContentFolder);
            _initialized = false;
        }          

        public static void AddLocaleText(string textId, string text)
        {
            GameManager.instance.localizationManager.activeDictionary.Add(textId, text);
        }

        public static void SaveByLaws(Entity[] bylawEntities, EntityManager em)
        {
            if (!_initialized)
            {
                Mod.log.Error("Utils Data isn't initialized! Not saving.");
                return;
            }
            var records = new List<ByLawRecord>();
            for(int i = 0; i < bylawEntities.Length; i++)
            {
                var entity = bylawEntities[i];
                var zonePrefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(entity);
                var zoneData = em.GetComponentData<ByLawZoneData>(entity);
                var prefabID = zonePrefab.GetPrefabID();
                if (zoneData.deleted) continue;
                records.Add(new ByLawRecord(zonePrefab.bylawName, zoneData.CreateDescription(), zonePrefab.m_Color, zonePrefab.m_Edge, zoneData, prefabID));
            }
            var toDump = records.ToArray();
            var path = Path.Combine(ContentFolder, "ZoningByLaws.json");
            File.WriteAllText(path, JSON.Dump(toDump));
        }

        public static bool GetByLawsFromFile(out ByLawRecord[] records)
        {
            var path = Path.Combine(ContentFolder, "ZoningByLaws.json");
            if (!File.Exists(path))
            {
                records = new ByLawRecord[0];
                return true;
            }
            try
            {
                records = JSON.MakeInto<ByLawRecord[]>(JSON.Load(File.ReadAllText(path)));
                return true;
            } catch (Exception ex)
            {
                Mod.log.Error("Error reading bylaw records: " + ex.Message);               
            }
            records = new ByLawRecord[0];
            return false;
        }

        public static bool LoadByLaws()
        {
            if (!_initialized)
            {
                Mod.log.Error("Utils Data isn't initialized! Not loading.");
                return false;
            }
            if (!GetByLawsFromFile(out var records))
            {
                return false;
            }
            List<ByLawZonePrefab> prefabs = new();            
            for (int i = 0; i < records.Length; i++)
            {
                ByLawRecord record = records[i];
                ByLawZonePrefab re = CreateByLawPrefabFromData(record.bylawZoneData, i + 1, record.idName + '_' + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), record.bylawName);
                re.m_Edge = record.edgeColor;
                re.m_Color = record.zoneColor;
                re.name = record.idName;
                prefabs.Add(re);
            }            
            foreach(var prefab in prefabs)
            {
                if (!_prefabSystem.AddPrefab(prefab))
                {
                    Mod.log.Error($"Failed to add new zone prefab \"{prefab.bylawName}\"!");
                    return false;
                } else
                {
                    Mod.log.Info($"Added new zone prefab: \"{prefab.bylawName}\"!");
                }
            }
            return true;
        }

        public static bool InitData(ZonePrefab basePrefab, UIAssetCategoryPrefab baseCategoryPrefab, PrefabSystem prefabSystem)
        {
            _basePrefab = basePrefab;
            _prefabSystem = prefabSystem;
            _assetCategory = CreateAssetCategory(baseCategoryPrefab);
            Mod.log.Info("Asset Category Name: " + _assetCategory.name);
            _initialized = _assetCategory != null && _basePrefab != null && _prefabSystem != null;
            return _initialized;
        }

        public static UIAssetCategoryPrefab CreateAssetCategory(UIAssetCategoryPrefab baseCategoryPrefab)
        {
            Mod.log.Info($"Copying UI Asset Category {baseCategoryPrefab.name}");
            ComponentBase[] baseComponents = new ComponentBase[baseCategoryPrefab.components.Count];
            baseCategoryPrefab.components.CopyTo(baseComponents);

            UIAssetCategoryPrefab assetCategory = new UIAssetCategoryPrefab();
            assetCategory.m_Menu = baseCategoryPrefab.m_Menu;
            assetCategory.active = true;
            assetCategory.isDirty = true;
            assetCategory.name = "ByLawZones";            
            assetCategory.components.AddRange(baseComponents);
            var baseUIObj = baseCategoryPrefab.GetComponent<UIObject>();
            assetCategory.Remove<UIObject>();
            var newUIObj = ScriptableObject.CreateInstance<UIObject>();
            newUIObj.m_Priority = 60; // after extractor zones (should be end of the list)
            newUIObj.name = "ByLawZones";
            newUIObj.m_Icon = "coui://trejak_zbl/mod-icon-colour.svg";
            newUIObj.m_Group = baseUIObj.m_Group;
            newUIObj.active = true;
            assetCategory.AddComponentFrom(newUIObj);
            if (!_prefabSystem.AddPrefab(assetCategory))
            {
                Mod.log.Error($"Failed to add By Law Zones UI Category!");
                return null;
            }
            else
            {
                Utils.AddLocaleText($"SubServices.NAME[{assetCategory.name}]", "Zoning ByLaws");
                Utils.AddLocaleText($"Assets.SUB_SERVICE_DESCRIPTION[{assetCategory.name}]", "Custom contraint zones created by you, the player.");
                Mod.log.Info($"Added ByLawZones UI Category with Menu {assetCategory.name}");
                return assetCategory;
            }
        }

        public static ByLawZonePrefab CreateByLawPrefabFromData(ByLawZoneData data, int byLawNumber, string idName = null, string bylawName = null)
        {
            if (!_initialized)
            {
                Mod.log.Error("Utils Data isn't initialized! Not creating bylaw");
                return null;
            }
            ComponentBase[] baseComponents = new ComponentBase[_basePrefab.components.Count];
            _basePrefab.components.CopyTo(baseComponents);

            var prefab = new ByLawZonePrefab();
            bylawName = bylawName ?? "Zoning ByLaw " + byLawNumber;
            idName = idName ?? bylawName +'_' + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Copy over prefab data
            prefab.zoneType = data.zoneType;
            prefab.height = data.height;
            prefab.lotSize = data.lotSize;
            prefab.frontage = data.frontage;
            prefab.parking = data.parking;
            prefab.bylawName = bylawName;

            // Typical Zoning Stuff
            prefab.m_Office = (prefab.zoneType & ByLawZoneType.Office) != (ByLawZoneType)0;
            prefab.m_Color = Color.red;
            prefab.m_Edge = Color.black;
            prefab.m_AreaType = Game.Zones.AreaType.Residential;
            prefab.name = idName;
            prefab.isDirty = true;
            prefab.active = true;

            var groupAmbience = ScriptableObject.CreateInstance<GroupAmbience>();
            groupAmbience.m_AmbienceType = Game.Simulation.GroupAmbienceType.None;
            prefab.AddComponentFrom(groupAmbience);

            var zoneProps = ScriptableObject.CreateInstance<ZoneProperties>();
            zoneProps.m_ScaleResidentials = false;
            prefab.components.Add(zoneProps);

            prefab.components.AddRange(baseComponents);
            prefab.GetComponent<Unlockable>().m_RequireAll = new PrefabBase[0];
            prefab.GetComponent<Unlockable>().m_RequireAny = new PrefabBase[0];
            prefab.GetComponent<Unlockable>().m_IgnoreDependencies = true;
            prefab.Remove<ThemeObject>();


            var uiObj = _basePrefab.GetComponent<UIObject>();
            prefab.Remove<UIObject>();
            var newUIObj = ScriptableObject.CreateInstance<UIObject>();
            newUIObj.m_Icon = null;
            newUIObj.name = uiObj.name.Replace("NA Residential Medium", idName);
            newUIObj.m_Priority = uiObj.m_Priority;
            newUIObj.m_Group = _assetCategory;
            newUIObj.m_Icon = "coui://trejak_zbl/config-icon.svg";
            //newUIObj.m_Group = uiObj.m_Group;
            newUIObj.active = uiObj.active;            
            prefab.AddComponentFrom(newUIObj);
            SetPrefabText(prefab, data);
            return prefab;
        }

        public static void SetPrefabText(ByLawZonePrefab prefab, ByLawZoneData data)
        {
            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.bylawName);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", data.CreateDescription());
        }

    }
}
