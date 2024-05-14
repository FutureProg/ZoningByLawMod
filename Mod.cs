using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI.Binding;
using Game;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.SceneFlow;
using Game.Simulation;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using ZoningByLaw;
using ZoningByLaw.Prefab;

namespace Trejak.ZoningByLaw;

public class Mod : IMod
{
    public static ILog log = LogManager.GetLogger($"{nameof(ZoningByLaw)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
    private Setting m_Setting;

    bool installed;
    private PrefabSystem _prefabSystem;

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            log.Info($"Current mod asset at {asset.path}");

        _prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
        var prefabs = Traverse.Create(_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
        var basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
        ComponentBase[] baseComponents = new ComponentBase[basePrefab.components.Count];
        basePrefab.components.CopyTo(baseComponents);

        var prefab = new ByLawZonePrefab();
        prefab.m_Office = (prefab.zoneType & ByLawZoneType.Office) != (ByLawZoneType)0;        
        prefab.m_Color = Color.red;
        prefab.m_Edge = Color.black;
        prefab.m_AreaType = Game.Zones.AreaType.Residential;
        prefab.name = "ByLaw Zoning";
        prefab.isDirty = true;
        prefab.active = true;        
        prefab.components.AddRange(baseComponents);

        var uiObj = prefab.GetComponent<UIObject>();
        prefab.Remove<UIObject>();
        var newUIObj = ScriptableObject.CreateInstance<UIObject>();
        newUIObj.m_Icon = null;
        newUIObj.name = uiObj.name.Replace("NA Residential Medium", "ByLaw Zone");
        newUIObj.m_Priority = uiObj.m_Priority;
        newUIObj.m_Group = uiObj.m_Group;
        newUIObj.active = uiObj.active;
        prefab.AddComponentFrom(newUIObj);


        if(!_prefabSystem.AddPrefab(prefab))
        {
            Mod.log.Error("Failed to add cloned zone prefab! exiting.");
            return;
        }

        GameManager.instance.onGameLoadingComplete += onGameLoadingComplete;

        updateSystem.UpdateAfter<ByLawZoneSpawnSystem, ZoneSpawnSystem>(SystemUpdatePhase.GameSimulation);

        //m_Setting = new Setting(this);
        //m_Setting.RegisterInOptionsUI();
        //GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

        //AssetDatabase.global.LoadSettings(nameof(ZoningByLaw), m_Setting, new Setting(this));
    }

    private void onGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
        if (installed)
        {
            return;
        }
        if (mode != GameMode.Game && mode != Game.GameMode.Editor)
        {
            return;
        }

        var prefabs = Traverse.Create(_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
        if (prefabs == null || !prefabs.Any())
        {
            Mod.log.Error($"Failed retrieving Prefabs list, exiting.");
            return;
        }
        installed = true;
    }

    public void OnDispose()
    {
        log.Info(nameof(OnDispose));
        if (m_Setting != null)
        {
            m_Setting.UnregisterInOptionsUI();
            m_Setting = null;
        }
    }
}
