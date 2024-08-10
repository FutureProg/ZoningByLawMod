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
    public partial class ByLawRenderPreviewSystem : GameSystemBase
    {

        private ZoningByLawToolSystem _bylawToolSystem;
        private OverlayRenderSystem _overlayRenderSystem;
        private ToolSystem _toolSystem;
        private GizmosSystem _gizmosSystem;

        private ConstraintData _constraintData;
        private readonly ConstraintData DefaultConstraints = new()
        {
            frontage = new() { max = 48, min = 0 },
            height = new() { max = 500, min = 0 },
            lotSize = new() { max = 2304, min = 0 },
            frontSetback = new() { max = 48, min = 0 }
        };

        public float3? drawPosition { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
            _bylawToolSystem = World.GetOrCreateSystemManaged<ZoningByLawToolSystem>();
            _gizmosSystem = World.GetOrCreateSystemManaged<GizmosSystem>();
            _constraintData = DefaultConstraints;
            this.Enabled = false;
        }        

        public void SetConstraintData(ByLawItem[] constraints)
        {
            this._constraintData = DefaultConstraints;
            foreach(ByLawItem item in constraints)
            {
                switch(item.byLawItemType)
                {
                    case ByLawItemType.Height:
                        _constraintData.height = item.valueBounds1;
                        break;
                    case ByLawItemType.LotWidth:
                        _constraintData.frontage = item.valueBounds1;
                        break;
                    case ByLawItemType.LotSize:
                        _constraintData.lotSize = item.valueBounds1;
                        break;
                    case ByLawItemType.FrontSetback:
                        _constraintData.frontSetback = item.valueBounds1;
                        break;
                }
            }
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
            var byLawZoneData = _bylawToolSystem.byLawZoneData;
            if (drawPosition.HasValue)
            {
                var renderPreviewJob = new RenderPreviewJob()
                {
                    overlayBuffer = _overlayRenderSystem.GetBuffer(out var overlayRenderHandle),
                    terrainPoint = drawPosition.Value,
                    constraintData = _constraintData,
                    gizmoBatcher = _gizmosSystem.GetGizmosBatcher(out var gizmoJobHandle)
                };
                this.Dependency = renderPreviewJob.Schedule(JobHandle.CombineDependencies(this.Dependency, overlayRenderHandle, gizmoJobHandle));
                _overlayRenderSystem.AddBufferWriter(this.Dependency);
            }
        }

        public struct ConstraintData
        {
            public Bounds1 lotSize;
            public Bounds1 frontage;
            public Bounds1 height;
            public Bounds1 frontSetback; // currently unused
        }

#if BURST    
        [BurstCompile]
#endif
        public partial struct RenderPreviewJob : IJob
        {

            public ConstraintData constraintData;
            public OverlayRenderSystem.Buffer overlayBuffer;
            public float3 terrainPoint;
            public GizmoBatcher gizmoBatcher;

            public void Execute()
            {
                float unitSizeMetres = 8f;
                float maxZoneSizeUnits = 6f;



                float width = math.min(unitSizeMetres * maxZoneSizeUnits, constraintData.frontage.max > 0 ? constraintData.frontage.max : unitSizeMetres * maxZoneSizeUnits);
                float height = math.min(50.0f, constraintData.height.max > 0 ? constraintData.height.max : 50.0f);

                float depth = 6 * unitSizeMetres;
                float lotSize = depth * width;
                while (true)
                {
                    bool passMinLotSize = constraintData.lotSize.min <= 0 || constraintData.lotSize.min <= lotSize;
                    bool passMaxLotSize = constraintData.lotSize.max <= 0 || constraintData.lotSize.max >= lotSize;
                    if (passMinLotSize && passMaxLotSize)
                    {
                        break;
                    }

                    // Basically set a value that isn't possible
                    if ((passMinLotSize || passMaxLotSize) && constraintData.lotSize.max - constraintData.lotSize.min < unitSizeMetres)
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
