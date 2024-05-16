using Colossal.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Common;
using Game.Prefabs;
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

namespace Trejak.ZoningByLaw
{
    public partial class ConfigPanelUISystem : UISystemBase
    {

        private PrefabSystem _prefabSystem;
        private EntityQuery _bylawsQuery;
        private Entity _selectedByLaw; // the prefab entity for the selected bylaw
        private PrefabBase _basePrefab; // the prefab we use to clone the zones

        private ValueBinding<ByLawZoneData> _selectedByLawData;
        private ValueBinding<ByLawZoneListItem[]> _byLawZoneList;

        private TriggerBinding<Entity> _setActiveByLaw;
        private TriggerBinding<ByLawZoneData> _setByLawData;
        private TriggerBinding<ByLawZoneData> _createNewByLaw;
        private TriggerBinding<Entity> _deleteByLaw;

        const string uiGroupName = "Trejak.ZoningByLaw";

        struct ByLawZoneListItem : IJsonWritable
        {
            public Entity entity;
            public string name;

            public void Write(IJsonWriter writer)
            {
                writer.TypeBegin(GetType().FullName);
                writer.Write("entity");
                writer.Write(entity);
                writer.Write("name");
                writer.Write(name);
                writer.TypeEnd();
            }
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 32;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _bylawsQuery = GetEntityQuery(ComponentType.ReadOnly<ByLawZoneData>());
            _selectedByLaw = Entity.Null;
            this.AddBinding(_selectedByLawData = new ValueBinding<ByLawZoneData>(uiGroupName, "SelectedByLawData", default));
            this.AddBinding(_byLawZoneList = new ValueBinding<ByLawZoneListItem[]>(uiGroupName, "ByLawZoneList", GetByLawList(), new ArrayWriter<ByLawZoneListItem>()));

            this.AddBinding(_setActiveByLaw = new TriggerBinding<Entity>(uiGroupName, "SetActiveByLaw", SetActiveByLaw));
            this.AddBinding(_setByLawData = new TriggerBinding<ByLawZoneData>(uiGroupName, "SetByLawData", SetByLawData));
            this.AddBinding(_createNewByLaw = new TriggerBinding<ByLawZoneData>(uiGroupName, "CreateNewByLaw", CreateNewByLaw));
            this.AddBinding(_deleteByLaw = new TriggerBinding<Entity>(uiGroupName, "DeleteByLaw", DeleteByLaw));

            _prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
            GetBasePrefab();
        }

        void SetActiveByLaw(Entity entity)
        {
            _selectedByLaw = entity;
            ByLawZoneData data;
            if (_selectedByLaw == Entity.Null)
            {
                data = default;
            } else
            {
                data = EntityManager.GetComponentData<ByLawZoneData>(entity);
            }            
            this._selectedByLawData.Update(data);
        }

        void SetByLawData(ByLawZoneData data)
        {
            EntityManager.SetComponentData<ByLawZoneData>(_selectedByLaw, data);
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
            string byLawName = "Zoning ByLaw #" + _bylawsQuery.CalculateEntityCount() + 1; // TODO: make it so the player can create their own name
            
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
            newUIObj.name = uiObj.name.Replace("NA Residential Medium", byLawName);
            newUIObj.m_Priority = uiObj.m_Priority;
            newUIObj.m_Group = uiObj.m_Group;
            newUIObj.active = uiObj.active;
            prefab.AddComponentFrom(newUIObj);
            if (!_prefabSystem.AddPrefab(prefab))
            {
                Mod.log.Error($"Failed to add new zone prefab \"{byLawName}\"!");
                return;
            }
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

        protected override void OnUpdate()
        {
            base.OnUpdate();
            UpdateByLawList();
        }

    }
}
