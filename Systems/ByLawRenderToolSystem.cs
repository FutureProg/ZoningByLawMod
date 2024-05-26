using Colossal.Mathematics;
using Game;
using Game.Common;
using Game.Input;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class ByLawRenderToolSystem : ToolBaseSystem
    {

        public ByLawZoneData? byLawZoneData;
        public float3? currentDrawPoint;

        public override string toolID => "Zoning ByLaw Render Tool";

        private TerrainSystem _terrainSystem;
        private OverlayRenderSystem _overlayRenderSystem;

        private ProxyAction _applyAction;

        protected override void OnCreate()
        {
            base.OnCreate();
            _terrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();

            _applyAction = InputManager.instance.FindAction("Tool", "Apply");

            Enabled = false;            
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _applyAction.shouldBeEnabled = true;
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            _applyAction.shouldBeEnabled = false;
            currentDrawPoint = null;
            byLawZoneData = null;
        }

        public override PrefabBase GetPrefab()
        {
            return null;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.collisionMask = (CollisionMask.OnGround | CollisionMask.Overground);
            m_ToolRaycastSystem.typeMask = TypeMask.Terrain;            
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (byLawZoneData.HasValue)
            {                                
                if (_applyAction.WasPressedThisFrame())
                {
                    if (GetRaycastResult(out var hit))
                    {
                        currentDrawPoint = hit.m_HitPosition;                        
                    } else
                    {
                        currentDrawPoint = null;
                    }                    
                    _terrainSystem.AddCPUHeightReader(inputDeps);
                    Mod.log.Info("Clicked at " + (currentDrawPoint?.ToString() ?? "null"));
                }
            }            
            return base.OnUpdate(inputDeps);
        }

        public void SetByLaw(ByLawZoneData bylawData)
        {
            this.byLawZoneData = bylawData;
            if (m_ToolSystem.activeTool != this)
            {
                SetToolEnabled(true);
            }
        }

        public void SetToolEnabled(bool isEnabled)
        {
            if (isEnabled && m_ToolSystem.activeTool != this)
            {
                m_ToolSystem.selected = Entity.Null;
                m_ToolSystem.activeTool = this;
            } else if (!isEnabled && m_ToolSystem.activeTool == this)
            {
                m_ToolSystem.selected = Entity.Null;
                m_ToolSystem.activeTool = m_DefaultToolSystem;
            }
        }        
    }
}
