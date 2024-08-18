using Colossal.IO.AssetDatabase.Internal;
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
using Trejak.ZoningByLaw.UI;
using Trejak.ZoningByLaw.UISystems;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class ZoningByLawToolSystem : ToolBaseSystem
    {

        public enum State
        {
            None,
            Default,
            PlopPreview,
            PreviewRunning
        }

        public ZoningByLawBinding? byLawZoneData;
        public State state { 
            get; 
            private set; 
        }

        public override string toolID => "Zoning ByLaw Tool";

        private TerrainSystem _terrainSystem;
        private ByLawRenderPreviewSystem _previewRenderSystem;
        private ConfigPanelUISystem _bylawUISystem;
        private ProxyAction _applyAction;

        protected override void OnCreate()
        {
            base.OnCreate();
            _terrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            _previewRenderSystem = World.GetOrCreateSystemManaged<ByLawRenderPreviewSystem>();

            _applyAction = InputManager.instance.FindAction("Tool", "Apply");
            _bylawUISystem = World.GetOrCreateSystemManaged<ConfigPanelUISystem>();
            

            Enabled = false;            
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();            
            this.SetState(State.Default);
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();            
            this.SetState(State.None);
        }

        public override PrefabBase GetPrefab()
        {
            return null;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            return false;
        }

        public void SetState(State newState)
        {
            this.state = newState;
            switch (state)
            {
                case State.None:
                    _applyAction.shouldBeEnabled = false;
                    _previewRenderSystem.Enabled = false;
                    _previewRenderSystem.drawPosition = null;
                    byLawZoneData = null;
                    _bylawUISystem.SetConfigPanelOpen(false);
                    break;
                case State.Default:
                    _applyAction.shouldBeEnabled = false;
                    _bylawUISystem.SetConfigPanelOpen(true);
                    break;
                case State.PlopPreview:
                    _applyAction.shouldBeEnabled = true;
                    break;
                case State.PreviewRunning:
                    _applyAction.shouldBeEnabled = false;
                    break;
                default:
                    break;
            }
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
                        _previewRenderSystem.drawPosition = hit.m_HitPosition;                        
                    } else
                    {
                        _previewRenderSystem.drawPosition = null;
                    }                    
                    _terrainSystem.AddCPUHeightReader(inputDeps);
                    Mod.log.Info("Clicked at " + (_previewRenderSystem.drawPosition?.ToString() ?? "null"));
                }
            }            
            return base.OnUpdate(inputDeps);
        }

        public void SetByLawData(ZoningByLawBinding bylawData)
        {
            this.byLawZoneData = bylawData;
            var constraints = bylawData.blocks[0].itemData;
            _previewRenderSystem.SetConstraintData(constraints);
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
