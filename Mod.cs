using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI.Binding;
using Game;
using Game.Buildings;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.UI;
using Colossal.Mathematics;
using Trejak.ZoningByLaw.Systems;
using Game.Rendering;

namespace Trejak.ZoningByLaw;

public class Mod : IMod
{
    public static ILog log = LogManager.GetLogger($"{nameof(ZoningByLaw)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
    private Setting m_Setting;

    bool installed;
    //private PrefabSystem _prefabSystem;

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            log.Info($"Current mod asset at {asset.path}");

        //_prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
        //var prefabs = Traverse.Create(_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
        //var basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
        //ComponentBase[] baseComponents = new ComponentBase[basePrefab.components.Count];
        //basePrefab.components.CopyTo(baseComponents);

        //var prefab = new ByLawZonePrefab();
        //prefab.zoneType = ByLawZoneType.Residential | ByLawZoneType.Commercial | ByLawZoneType.Office;
        //prefab.height = new Bounds1(0, 100);        

        //prefab.m_Office = (prefab.zoneType & ByLawZoneType.Office) != (ByLawZoneType)0;        
        //prefab.m_Color = Color.red;
        //prefab.m_Edge = Color.black;
        //prefab.m_AreaType = Game.Zones.AreaType.Residential;
        //prefab.name = "Zoning ByLaw #0";
        //prefab.isDirty = true;
        //prefab.active = true;        
        //prefab.components.AddRange(baseComponents);
        //prefab.Remove(typeof(Unlockable));

        //var uiObj = prefab.GetComponent<UIObject>();
        //prefab.Remove<UIObject>();
        //var newUIObj = ScriptableObject.CreateInstance<UIObject>();
        //newUIObj.m_Icon = null;
        //newUIObj.name = "Zoning ByLaw #0";//uiObj.name.Replace("NA Residential Medium", "ByLaw Zone");
        //newUIObj.m_Priority = uiObj.m_Priority;
        //newUIObj.m_Group = uiObj.m_Group;
        //newUIObj.active = uiObj.active;      
        //prefab.AddComponentFrom(newUIObj);


        //if(!_prefabSystem.AddPrefab(prefab))
        //{
        //    Mod.log.Error("Failed to add cloned zone prefab! exiting.");
        //    return;
        //}
        //Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.name);
        //Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", prefab.CreateDescription());

        installed = false;
        GameManager.instance.onGameLoadingComplete += onGameLoadingComplete;
        GameManager.instance.onGamePreload += Instance_onGamePreload;        

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ZoneCheckSystem>().Enabled = false;
        updateSystem.UpdateAfter<ByLawZoneSpawnSystem, ZoneSpawnSystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<ByLawZonePrefabInitSystem>(SystemUpdatePhase.PrefabUpdate);
        updateSystem.UpdateAt<ConfigPanelUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<ResetGameToolbarUISystem>(SystemUpdatePhase.Modification1);
        updateSystem.UpdateAt<ByLawRenderToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAfter<ByLawRenderOverlaySystem, AreaRenderSystem>(SystemUpdatePhase.Rendering);

        var prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
        var prefabs = Traverse.Create(prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
        var basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
        Utils.InitData(basePrefab as ZonePrefab, prefabSystem);

        //m_Setting = new Setting(this);
        //m_Setting.RegisterInOptionsUI();
        //GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

        //AssetDatabase.global.LoadSettings(nameof(ZoningByLaw), m_Setting, new Setting(this));
        Utils.LoadByLaws();
        installed = true;
    }

    private void Instance_onGamePreload(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
        if (mode == GameMode.Game && !installed)
        {                        
        }        
    }

    private void onGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
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
