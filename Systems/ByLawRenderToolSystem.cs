using Colossal.Mathematics;
using Game;
using Game.Common;
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
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class ByLawRenderToolSystem : ToolBaseSystem
    {

        public ByLawZoneData? byLawZoneData;
        public override string toolID => "Zoning ByLaw Render Tool";

        private TerrainSystem _terrainSystem;
        private OverlayRenderSystem _overlayRenderSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _terrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            Enabled = false;            
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
                if (GetRaycastResult(out var hit))
                {
                    var hitPos = hit.m_HitPosition;
                    var renderPreviewJob = new RenderPreviewJob()
                    {
                        overlayBuffer = _overlayRenderSystem.GetBuffer(out var overlayDependencies),
                        terraintPoint = hitPos,
                        bylawData = byLawZoneData.Value
                    };
                    inputDeps = renderPreviewJob.Schedule(JobHandle.CombineDependencies(inputDeps, overlayDependencies));
                    _overlayRenderSystem.AddBufferWriter(inputDeps);
                }                
                _terrainSystem.AddCPUHeightReader(inputDeps);
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
                m_ToolSystem.activeTool = null;
            }
        }

        public partial struct RenderPreviewJob : IJob
        {

            public ByLawZoneData bylawData;
            public OverlayRenderSystem.Buffer overlayBuffer;
            public float3 terraintPoint;

            public void Execute()
            {
                float unitSizeMetres = 8f;
                float maxZoneSizeUnits = 6f;

                float width = math.min(unitSizeMetres * maxZoneSizeUnits, bylawData.frontage.max > 0 ? bylawData.frontage.max : unitSizeMetres * maxZoneSizeUnits);
                float height = math.min(100.0f, bylawData.height.max > 0 ? bylawData.height.max : 100.0f);

                float depth = 6 * unitSizeMetres;
                float lotSize = depth * width;                
                while(true)
                {
                    bool passMinLotSize = bylawData.lotSize.min <= 0 || bylawData.lotSize.min <= lotSize;
                    bool passMaxLotSize = bylawData.lotSize.max <= 0 || bylawData.lotSize.max >= lotSize;
                    if (passMinLotSize && passMaxLotSize)
                    {
                        break;
                    }

                    // Basically set a value that isn't possible
                    if ((passMinLotSize || passMaxLotSize) && bylawData.lotSize.max - bylawData.lotSize.min < unitSizeMetres)
                    {
                        break;
                    }
                    if (!passMinLotSize)
                    {
                        depth += unitSizeMetres;
                    }
                    if (!passMaxLotSize)
                    {
                        depth -= unitSizeMetres;
                    }

                    lotSize = depth * width;
                }
                // x and z horizontal plane
                float x = width / 2f - terraintPoint.x;
                float z = depth / 2f - terraintPoint.z;
                float y = height + terraintPoint.y;

                Bounds3 bounds = new Bounds3(new float3(x, terraintPoint.y, z), new float3(x + width, y, z + depth));

                // for now just draw a line of the correct height. A cube is a lot of work.
                Color lineColor = Color.cyan;
                float lineWidth = 0.5f;
                overlayBuffer.DrawLine(lineColor, new Line3.Segment(terraintPoint, new float3(terraintPoint.x, y, terraintPoint.z)), lineWidth);

            }
        }
    }
}
