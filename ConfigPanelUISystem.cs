using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Input;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Trejak.ZoningByLaw.UI
{
    public partial class ConfigPanelUISystem : UISystemBase
    {

        private PrefabSystem _prefabSystem;
        private EntityQuery _bylawsQuery;
        private Entity _selectedByLaw; // the prefab entity for the selected bylaw
        private PrefabBase _basePrefab; // the prefab we use to clone the zones

        private ValueBinding<ByLawZoneData> _selectedByLawData;
        private ValueBinding<ByLawZoneListItem[]> _byLawZoneList;
        private ValueBinding<bool> _configPanelOpen;
        private ValueBinding<string> _selectedByLawName;
        private ValueBinding<Color[]> _selectedByLawColour;

        private TriggerBinding<Entity> _setActiveByLaw;
        private TriggerBinding<ByLawZoneData> _setByLawData;
        private TriggerBinding<ByLawZoneData> _createNewByLaw;
        private TriggerBinding<Entity> _deleteByLaw;
        private TriggerBinding<bool> _setConfigPanelOpen;
        private TriggerBinding<string> _setByLawName;
        private TriggerBinding<Color, Color> _setByLawZoneColour;        

        const string uiGroupName = "Trejak.ZoningByLaw";

        struct ByLawZoneListItem : IJsonWritable
        {
            public Entity entity;
            public string name;

            public void Write(IJsonWriter writer)
            {
                writer.TypeBegin(GetType().FullName);
                writer.PropertyName("entity");
                writer.Write(entity);
                writer.PropertyName("name");
                writer.Write(name);
                writer.TypeEnd();
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _bylawsQuery = GetEntityQuery(ComponentType.ReadOnly<ByLawZoneData>());
            _selectedByLaw = Entity.Null;
            _prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
            GetBasePrefab();

            this.AddBinding(_selectedByLawData = new ValueBinding<ByLawZoneData>(uiGroupName, "SelectedByLawData", default));
            this.AddBinding(_configPanelOpen = new ValueBinding<bool>(uiGroupName, "IsConfigPanelOpen", false));
            this.AddBinding(_selectedByLawName = new ValueBinding<string>(uiGroupName, "SelectedByLawName", ""));
            this.AddBinding(_byLawZoneList = new ValueBinding<ByLawZoneListItem[]>(uiGroupName, "ByLawZoneList", GetByLawList(), new ArrayWriter<ByLawZoneListItem>()));
            this.AddBinding(_selectedByLawColour = new ValueBinding<Color[]>(uiGroupName, "SelectedByLawColour", new Color[] { default, default }, new ArrayWriter<Color>()));

            this.AddBinding(_setActiveByLaw = new TriggerBinding<Entity>(uiGroupName, "SetActiveByLaw", SetActiveByLaw));
            this.AddBinding(_setByLawData = new TriggerBinding<ByLawZoneData>(uiGroupName, "SetByLawData", SetByLawData));
            this.AddBinding(_createNewByLaw = new TriggerBinding<ByLawZoneData>(uiGroupName, "CreateNewByLaw", CreateNewByLaw));
            this.AddBinding(_deleteByLaw = new TriggerBinding<Entity>(uiGroupName, "DeleteByLaw", DeleteByLaw));
            this.AddBinding(_setConfigPanelOpen = new TriggerBinding<bool>(uiGroupName, "SetConfigPanelOpen", SetConfigPanelOpen));
            this.AddBinding(_setByLawName = new TriggerBinding<string>(uiGroupName, "SetByLawName", SetByLawName));
            this.AddBinding(_setByLawZoneColour = new TriggerBinding<Color, Color>(uiGroupName, "SetByLawZoneColour", SetByLawZoneColour));
        }

        void SetByLawZoneColour(Color zoneColour, Color borderColour)
        {
            if (_selectedByLaw == Entity.Null)
            {
                return;
            }
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw);
            prefab.m_Color = zoneColour;
            prefab.m_Edge = borderColour;
            _selectedByLawColour.Update(new Color[] { zoneColour, borderColour });
        }

        void SetConfigPanelOpen(bool newValue)
        {
            _configPanelOpen.Update(newValue);
        }

        void SetActiveByLaw(Entity entity)
        {
            Mod.log.Info("Set active by law to " + entity.Index + ", " + entity.Version);
            _selectedByLaw = entity;
            ByLawZoneData data;
            if (_selectedByLaw == Entity.Null)
            {
                data = default;
            } else
            {
                data = EntityManager.GetComponentData<ByLawZoneData>(entity);
            }
            bool result = _prefabSystem.TryGetPrefab<ByLawZonePrefab>(entity, out var prefab);            
            this._selectedByLawData.Update(data);
            if (result)
            {                
                this._selectedByLawColour.Update(new Color[] { prefab.m_Color, prefab.m_Edge });
                this._selectedByLawName.Update(prefab.name != null? prefab.name : "");
            } else
            {
                this._selectedByLawColour.Update(new Color[] { default, default });
                this._selectedByLawName.Update("");
            }
        }

        void SetByLawData(ByLawZoneData data)
        {
            Mod.log.Info("Set By Law Data: " + data);
            EntityManager.SetComponentData<ByLawZoneData>(_selectedByLaw, data);
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", data.CreateDescription());
            this._selectedByLawData.Update(data);
        }

        void SetByLawName(string name)
        {
            if (_selectedByLaw == Entity.Null)
            {
                return;
            }            
            var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(_selectedByLaw);
            prefab.name = name;
            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.name);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", _selectedByLawData.value.CreateDescription());
            _selectedByLawName.Update(name);
            UpdateByLawList();
        }

        void GetBasePrefab()
        {
            var prefabs = Traverse.Create(_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            _basePrefab = prefabs.FirstOrDefault(p => p.name == "NA Residential Medium");
        }

        /// <summary>
        /// Called from the UI after the user hits save on a new bylaw
        /// </summary>
        /// <param name="data"></param>
        void CreateNewByLaw(ByLawZoneData data)
        {            
            if (_basePrefab == null)
            {
                GetBasePrefab();
            }
            ComponentBase[] baseComponents = new ComponentBase[_basePrefab.components.Count];
            _basePrefab.components.CopyTo(baseComponents);

            var prefab = new ByLawZonePrefab();
            string byLawName = "Zoning ByLaw #" + _bylawsQuery.CalculateEntityCount(); // TODO: make it so the player can create their own name
            
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
            prefab.components.AddRange(baseComponents);

            var uiObj = prefab.GetComponent<UIObject>();
            prefab.Remove<UIObject>();
            var newUIObj = ScriptableObject.CreateInstance<UIObject>();
            newUIObj.m_Icon = null;
            newUIObj.name = byLawName;//uiObj.name.Replace("NA Residential Medium", byLawName);
            newUIObj.m_Priority = uiObj.m_Priority;
            newUIObj.m_Group = uiObj.m_Group;
            newUIObj.active = uiObj.active;
            prefab.AddComponentFrom(newUIObj);
            if (!_prefabSystem.AddPrefab(prefab))
            {
                Mod.log.Error($"Failed to add new zone prefab \"{byLawName}\"!");
                return;
            }
            Utils.AddLocaleText($"Assets.NAME[{prefab.name}]", prefab.name);
            Utils.AddLocaleText($"Assets.DESCRIPTION[{prefab.name}]", data.CreateDescription());
            UpdateByLawList();
        }

        void DeleteByLaw(Entity entity)
        {
            var data = EntityManager.GetComponentData<ByLawZoneData>(entity);
            data.deleted = true;
            EntityManager.SetComponentData(entity, data);            
            UpdateByLawList();
            SetActiveByLaw(Entity.Null);
        }

        void UpdateByLawList()
        {            
            this._byLawZoneList.Update(GetByLawList());
        }

        ByLawZoneListItem[] GetByLawList()
        {
            List<ByLawZoneListItem> list = new();
            var entityArr = _bylawsQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entityArr.Length; i++)
            {                
                var entity = entityArr[i];
                var data = EntityManager.GetComponentData<ByLawZoneData>(entity);
                var prefabData = EntityManager.GetComponentData<PrefabData>(entity);
                var prefab = _prefabSystem.GetPrefab<ByLawZonePrefab>(prefabData);
                if (data.deleted) continue;
                list.Add(new ByLawZoneListItem()
                {
                    entity = entity,
                    name = (i+1) + ": " + prefab.name
                });
            }
            entityArr.Dispose();
            return list.ToArray();
        }

    }
}
