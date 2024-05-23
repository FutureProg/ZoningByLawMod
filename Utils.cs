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

namespace Trejak.ZoningByLaw
{
    public static class Utils
    {

        private static PrefabBase _basePrefab;
        private static PrefabSystem _prefabSystem;
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
                ByLawZonePrefab re = CreateByLawPrefabFromData(record.bylawZoneData, i + 1, record.idName, record.bylawName);
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

        public static void InitData(ZonePrefab basePrefab, PrefabSystem prefabSystem)
        {
            _basePrefab = basePrefab;
            _prefabSystem = prefabSystem;
            _initialized = true;
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
            idName = idName ?? bylawName;

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

            var uiObj = _basePrefab.GetComponent<UIObject>();
            prefab.Remove<UIObject>();
            var newUIObj = ScriptableObject.CreateInstance<UIObject>();
            newUIObj.m_Icon = null;
            newUIObj.name = uiObj.name.Replace("NA Residential Medium", idName);
            newUIObj.m_Priority = uiObj.m_Priority;
            newUIObj.m_Group = uiObj.m_Group;
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
