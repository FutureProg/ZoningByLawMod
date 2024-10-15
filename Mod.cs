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
using System.Reflection;
using System.IO;
using Colossal.PSI.Environment;
using Colossal.IO.AssetDatabase.Internal;
using System.Text;
using Colossal.UI;
using Trejak.ZoningByLaw.Tests;
using Game.UI.InGame;
using Game.Settings;

namespace Trejak.ZoningByLaw;

public class Mod : IMod
{
    public static string Id = "Trejak.ZoningByLaw";
    public static ILog log = LogManager.GetLogger($"{nameof(ZoningByLaw)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
    private Setting m_Setting;

    bool installed;
    private Harmony _Harmony;    

    //private PrefabSystem _prefabSystem;

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            log.Info($"Current mod asset at {asset.path}");

        installed = false;        

        var path = Path.GetDirectoryName(asset.GetMeta().path);
        UIManager.defaultUISystem.AddHostLocation("trejak_zbl", Path.Combine(path, "Images/"));
        ApplyPatches();

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<ZoneCheckSystem>().Enabled = false;
        updateSystem.UpdateAfter<ByLawZoneSpawnSystem, ZoneSpawnSystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<ByLawZonePrefabInitSystem>(SystemUpdatePhase.PrefabUpdate);
        updateSystem.UpdateAt<IndexBuildingsSystem>(SystemUpdatePhase.PrefabUpdate);

        updateSystem.UpdateAt<ConfigPanelUISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<CountElligiblePropertiesSystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<ResetGameToolbarUISystem>(SystemUpdatePhase.Modification1);
        updateSystem.UpdateAt<ZoningByLawToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAfter<ByLawRenderPreviewSystem, AreaRenderSystem>(SystemUpdatePhase.Rendering);

        updateSystem.UpdateAt<BuildingPropertiesToolTipSystem>(SystemUpdatePhase.UITooltip);

        var prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
        var prefabs = Traverse.Create(prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
        var basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
        var baseCategoryPrefab = prefabs.FirstOrDefault(p => p.name.StartsWith("Zones") && p is UIAssetCategoryPrefab) as UIAssetCategoryPrefab;

        TestRunner.Run();
        if (!Utils.InitData(basePrefab as ZonePrefab, baseCategoryPrefab, prefabSystem) || TestRunner.FailureCount > 0)
        {
            Mod.log.Error("Unable to initialize Zoning ByLaw Mod!");            
        } else
        {            
            Utils.LoadByLaws();

            // init localization
            var em = updateSystem.EntityManager;
            LocaleDictionary localeDict = new LocaleDictionary(em, em.World.GetOrCreateSystemManaged<PrefabUISystem>(), em.World.GetOrCreateSystemManaged<PrefabSystem>());
            foreach (var localeId in GameManager.instance.localizationManager.GetSupportedLocales())
            {
                GameManager.instance.localizationManager.AddSource(localeId, localeDict);
            }

            installed = true;
        }

        //m_Setting = new Setting(this);
        //m_Setting.RegisterInOptionsUI();
        //GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

        //AssetDatabase.global.LoadSettings(nameof(ZoningByLaw), m_Setting, new Setting(this));

    }

    private void ApplyPatches()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        _Harmony = new Harmony(typeof(Mod).Namespace);
        _Harmony.PatchAll(assembly);
        var patchedMethods = _Harmony.GetPatchedMethods().ToArray<MethodBase>();

        log.Info($"Made patches! Patched methods: " + patchedMethods.Length);

        foreach (var patchedMethod in patchedMethods)
        {
            log.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
        }
    }

    public static InterfaceSettings GameInterfaceSettings
    {
        get
        {
            return GameManager.instance?.settings?.userInterface;
        }
    }

    public static InterfaceSettings.UnitSystem GameMeasurementUnitSystem
    {
        get
        {
            return GameInterfaceSettings?.unitSystem ?? InterfaceSettings.UnitSystem.Metric;
        }
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
