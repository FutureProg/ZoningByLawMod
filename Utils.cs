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

        public static string ContentFolder { get; }

        static Utils()
        {
            ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", "Trejak.ZoningByLaw");
            Directory.CreateDirectory(ContentFolder);
        }          

        public static void AddLocaleText(string textId, string text)
        {
            GameManager.instance.localizationManager.activeDictionary.Add(textId, text);
        }

        public static void SaveByLaws(Entity[] bylawEntities, EntityManager em)
        {
            var prefabSys = GetPrefabSystem();
            var records = new List<ByLawRecord>();
            for(int i = 0; i < bylawEntities.Length; i++)
            {
                var entity = bylawEntities[i];
                var zonePrefab = prefabSys.GetPrefab<ByLawZonePrefab>(entity);
                var zoneData = em.GetComponentData<ByLawZoneData>(entity);
                if (zoneData.deleted) continue;
                records.Add(new ByLawRecord(zonePrefab.name, zoneData.CreateDescription(), zoneData));
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
            if (!GetByLawsFromFile(out var records))
            {
                return false;
            }
            List<ByLawZonePrefab> prefabs = new();            
            for (int i = 0; i < records.Length; i++)
            {
                ByLawRecord record = records[i];
                ByLawZonePrefab re = CreateByLawPrefabFromData(record.bylawZoneData, i + 1, record.bylawName);
                prefabs.Add(re);
            }
            var prefabSystem = GetPrefabSystem();
            foreach(var prefab in prefabs)
            {
                if (!prefabSystem.AddPrefab(prefab))
                {
                    Mod.log.Error($"Failed to add new zone prefab \"{prefab.name}\"!");
                    return false;
                }
            }            
            return true;
        }

        public static PrefabBase GetBasePrefab()
        {
            if (_basePrefab != null)
            {
                return _basePrefab;
            }
            var prefabSystem = GetPrefabSystem();
            var prefabs = Traverse.Create(prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            _basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
            return _basePrefab;         
        }

        static PrefabSystem GetPrefabSystem()
        {
            if (_prefabSystem == null)
            {
                return World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
            }
            return _prefabSystem;
        }

        public static ByLawZonePrefab CreateByLawPrefabFromData(ByLawZoneData data, int byLawNumber, string bylawName = null)
        {
            var basePrefab = GetBasePrefab();
            ComponentBase[] baseComponents = new ComponentBase[basePrefab.components.Count];
            basePrefab.components.CopyTo(baseComponents);

            var prefab = new ByLawZonePrefab();
            string byLawName = bylawName ?? "Zoning ByLaw #" + byLawNumber;

            // Copy over prefab data
            prefab.zoneType = data.zoneType;
            prefab.height = data.height;
            prefab.lotSize = data.lotSize;
            prefab.frontage = data.frontage;
            prefab.parking = data.parking;

            // Typical Zoning Stuff
            prefab.m_Office = (prefab.zoneType & ByLawZoneType.Office) != (ByLawZoneType)0;
            prefab.m_Color = Color.red;
            prefab.m_Edge = Color.black;
            prefab.m_AreaType = Game.Zones.AreaType.Residential;
            prefab.name = byLawName;
            prefab.isDirty = true;
            prefab.active = true;

            var groupAmbience = ScriptableObject.CreateInstance<GroupAmbience>();
            groupAmbience.m_AmbienceType = Game.Simulation.GroupAmbienceType.None;
            prefab.AddComponentFrom(groupAmbience);

            var zoneProps = ScriptableObject.CreateInstance<ZoneProperties>();
            zoneProps.m_ScaleResidentials = false;
            prefab.components.Add(zoneProps);

            //prefab.components.AddRange(baseComponents);

            var uiObj = basePrefab.GetComponent<UIObject>();
            prefab.Remove<UIObject>();
            var newUIObj = ScriptableObject.CreateInstance<UIObject>();
            newUIObj.m_Icon = null;
            newUIObj.name = byLawName;//uiObj.name.Replace("NA Residential Medium", byLawName);
            newUIObj.m_Priority = uiObj.m_Priority;
            newUIObj.m_Group = uiObj.m_Group;
            newUIObj.active = uiObj.active;
            prefab.AddComponentFrom(newUIObj);
            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.name);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", data.CreateDescription());
            return prefab;
        }

    }
}
