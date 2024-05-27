using Colossal;
using Colossal.Mathematics;
using Game;
using Game.Rendering;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Systems;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class ByLawRenderOverlaySystem : GameSystemBase
    {

        private ByLawRenderToolSystem _bylawRenderToolSystem;
        private OverlayRenderSystem _overlayRenderSystem;
        private ToolSystem _toolSystem;
        private GizmosSystem _gizmosSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            _bylawRenderToolSystem = World.GetOrCreateSystemManaged<ByLawRenderToolSystem>();
            _gizmosSystem = World.GetOrCreateSystemManaged<GizmosSystem>();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            if (_toolSystem.activeTool != _bylawRenderToolSystem || !_bylawRenderToolSystem.byLawZoneData.HasValue)
            {
                return;
            }
            var currentDrawPoint = _bylawRenderToolSystem.currentDrawPoint;
            var byLawZoneData = _bylawRenderToolSystem.byLawZoneData;
            if (currentDrawPoint.HasValue)
            {
                var renderPreviewJob = new RenderPreviewJob()
                {
                    overlayBuffer = _overlayRenderSystem.GetBuffer(out var overlayRenderHandle),
                    terrainPoint = currentDrawPoint.Value,
                    bylawData = byLawZoneData.Value,
                    gizmoBatcher = _gizmosSystem.GetGizmosBatcher(out var gizmoJobHandle)
                };
                this.Dependency = renderPreviewJob.Schedule(JobHandle.CombineDependencies(this.Dependency, overlayRenderHandle, gizmoJobHandle));
                _overlayRenderSystem.AddBufferWriter(this.Dependency);
            }
        }

#if BURST    
        [BurstCompile]
#endif
        public partial struct RenderPreviewJob : IJob
        {

            public ByLawZoneData bylawData;
            public OverlayRenderSystem.Buffer overlayBuffer;
            public float3 terrainPoint;
            public GizmoBatcher gizmoBatcher;

            public void Execute()
            {
                float unitSizeMetres = 8f;
                float maxZoneSizeUnits = 6f;

                float width = math.min(unitSizeMetres * maxZoneSizeUnits, bylawData.frontage.max > 0 ? bylawData.frontage.max : unitSizeMetres * maxZoneSizeUnits);
                float height = math.min(50.0f, bylawData.height.max > 0 ? bylawData.height.max : 50.0f);

                float depth = 6 * unitSizeMetres;
                float lotSize = depth * width;
                while (true)
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
                float x = terrainPoint.x - width / 2f;
                float z = terrainPoint.z - depth / 2f;
                float y = height + terrainPoint.y;                

                // for now just draw a line of the correct height. A cube is a lot of work.
                Color lineColor = Color.red;
                float lineWidth = 1.5f;
                overlayBuffer.DrawLine(lineColor, Color.clear, 
                    lineWidth, 0, 
                    new Line3.Segment(new float3(terrainPoint.x, terrainPoint.y, terrainPoint.z - depth/2f), new float3(terrainPoint.x, terrainPoint.y, terrainPoint.z + depth / 2f)), 
                    width
                ); // draws the lot size

                float3 cubeCentre = terrainPoint+ new float3(0, height / 2f, 0);
                float3 cubeScale = new float3(width, height, depth);
                gizmoBatcher.DrawWireCube(cubeCentre, cubeScale, lineColor); // draws the building
            }
        }

    }
}
