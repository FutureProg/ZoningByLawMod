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
using Trejak.ZoningByLaw.BuildingBlocks;
using Trejak.ZoningByLaw.Prefab;
using Trejak.ZoningByLaw.Systems;
using Trejak.ZoningByLaw.UISystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Trejak.ZoningByLaw.Systems
{
    public partial class ByLawRenderOverlaySystem : GameSystemBase
    {

        private ZoningByLawToolSystem _bylawToolSystem;
        private OverlayRenderSystem _overlayRenderSystem;
        private ToolSystem _toolSystem;
        private GizmosSystem _gizmosSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            _bylawToolSystem = World.GetOrCreateSystemManaged<ZoningByLawToolSystem>();
            _gizmosSystem = World.GetOrCreateSystemManaged<GizmosSystem>();
            this.Enabled = false;
        }

        public void SetConstraintData(NativeArray<ByLawItem> constraints)
        {
            //TODO: Create function that returns the constraint data
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        protected override void OnUpdate()
        {
            if (_toolSystem.activeTool != _bylawToolSystem || !_bylawToolSystem.byLawZoneData.HasValue)
            {
                return;
            }
            var currentDrawPoint = _bylawToolSystem.currentDrawPoint;
            var byLawZoneData = _bylawToolSystem.byLawZoneData;
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

        struct ConstraintData
        {
            public Bounds1 lotSizeConstraint;
            public Bounds1 frontage;
            public Bounds1 heightConstraint;
            public Bounds1 setback;
        }

#if BURST    
        [BurstCompile]
#endif
        public partial struct RenderPreviewJob : IJob
        {

            public Bounds1 lotSizeConstraint;
            public Bounds1 frontage;
            public Bounds1 heightConstraint;
            public Bounds1 setback;
            public OverlayRenderSystem.Buffer overlayBuffer;
            public float3 terrainPoint;
            public GizmoBatcher gizmoBatcher;

            public void Execute()
            {
                float unitSizeMetres = 8f;
                float maxZoneSizeUnits = 6f;



                float width = math.min(unitSizeMetres * maxZoneSizeUnits, frontage.max > 0 ? frontage.max : unitSizeMetres * maxZoneSizeUnits);
                float height = math.min(50.0f, heightConstraint.max > 0 ? heightConstraint.max : 50.0f);

                float depth = 6 * unitSizeMetres;
                float lotSize = depth * width;
                while (true)
                {
                    bool passMinLotSize = lotSizeConstraint.min <= 0 || lotSizeConstraint.min <= lotSize;
                    bool passMaxLotSize = lotSizeConstraint.max <= 0 || lotSizeConstraint.max >= lotSize;
                    if (passMinLotSize && passMaxLotSize)
                    {
                        break;
                    }

                    // Basically set a value that isn't possible
                    if ((passMinLotSize || passMaxLotSize) && lotSizeConstraint.max - lotSizeConstraint.min < unitSizeMetres)
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
