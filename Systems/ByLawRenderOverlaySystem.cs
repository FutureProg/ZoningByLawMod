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

        bool doOnce = true;

        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            _bylawRenderToolSystem = World.GetOrCreateSystemManaged<ByLawRenderToolSystem>();
            doOnce = true;
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            if (_toolSystem.activeTool != _bylawRenderToolSystem || !_bylawRenderToolSystem.byLawZoneData.HasValue)
            {
                doOnce = true;
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
                    bylawData = byLawZoneData.Value
                };
                this.Dependency = renderPreviewJob.Schedule(JobHandle.CombineDependencies(this.Dependency, overlayRenderHandle));
                _overlayRenderSystem.AddBufferWriter(this.Dependency);
                if (doOnce)
                {
                    Mod.log.Info("Rendering line with max height: " + math.min(100.0f, byLawZoneData.Value.height.max > 0 ? byLawZoneData.Value.height.max : 100.0f));
                    doOnce = false;
                }
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

            public void Execute()
            {
                float unitSizeMetres = 8f;
                float maxZoneSizeUnits = 6f;

                float width = math.min(unitSizeMetres * maxZoneSizeUnits, bylawData.frontage.max > 0 ? bylawData.frontage.max : unitSizeMetres * maxZoneSizeUnits);
                float height = math.min(50.0f, bylawData.height.max > 0 ? bylawData.height.max : 50.0f);

                float depth = 6 * unitSizeMetres;
                float lotSize = depth * width;
                //while (true)
                //{
                //    bool passMinLotSize = bylawData.lotSize.min <= 0 || bylawData.lotSize.min <= lotSize;
                //    bool passMaxLotSize = bylawData.lotSize.max <= 0 || bylawData.lotSize.max >= lotSize;
                //    if (passMinLotSize && passMaxLotSize)
                //    {
                //        break;
                //    }

                //    // Basically set a value that isn't possible
                //    if ((passMinLotSize || passMaxLotSize) && bylawData.lotSize.max - bylawData.lotSize.min < unitSizeMetres)
                //    {
                //        break;
                //    }
                //    if (!passMinLotSize)
                //    {
                //        depth += unitSizeMetres;
                //    }
                //    if (!passMaxLotSize)
                //    {
                //        depth -= unitSizeMetres;
                //    }

                //    lotSize = depth * width;
                //}
                // x and z horizontal plane
                float x = width / 2f - terrainPoint.x;
                float z = depth / 2f - terrainPoint.z;
                float y = height + terrainPoint.y;

                Bounds3 bounds = new Bounds3(new float3(x, terrainPoint.y, z), new float3(x + width, y, z + depth));

                // for now just draw a line of the correct height. A cube is a lot of work.
                Color lineColor = Color.red;
                float lineWidth = 4.5f;
                overlayBuffer.DrawLine(lineColor, lineColor, lineWidth, 0, new Line3.Segment(terrainPoint, new float3(terrainPoint.x + 0.01f, y, terrainPoint.z + 0.01f)), lineWidth, new float2(1.5f, 1.5f));
                overlayBuffer.DrawCircle(lineColor, Color.clear, lineWidth, 0, new float2(0, 0), terrainPoint + 0.1f, 50.0f);

            }
        }

    }
}
