using Colossal.Collections;
using Game;
using Game.Audio;
using Game.Common;
using Game.Input;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Trejak.ZoningByLaw.Tools;

public partial class ByLawZoneToolSystem : ToolBaseSystem
{
    public override string toolID => "ByLaw Zone Tool";

    public bool overwrite { get; set; }

    public ZonePrefab prefab
    {
        get
        {
            return this._prefab;
        }
        set
        {
            if (this._prefab != value)
            {
                this.m_ForceUpdate = true;
                this._prefab = value;
            }
        }
    }

    private ZonePrefab _prefab;
    private ToolOutputBarrier _toolOutputBarrier;
    AudioManager _audioManager;
    TerrainSystem _terrainSystem;
    
    EntityQuery _definitionGroup;
    EntityQuery _tempBlockQuery;
    EntityQuery _soundQuery;
    
    ProxyAction _applyAction;
    ProxyAction _secondaryApplyAction;

    NativeValue<ControlPoint> _snapPoint;
    private ControlPoint _raycastPoint;
    private ControlPoint _startPoint;

    private enum State
    {
        Default,           
        Zoning,
        Dezoning
    }
    private State _state;

    protected override void OnCreate()
    {
        base.OnCreate();
        this._toolOutputBarrier = base.World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        this._audioManager = base.World.GetOrCreateSystemManaged<AudioManager>();
        this._terrainSystem = base.World.GetOrCreateSystemManaged<TerrainSystem>();
        this._definitionGroup = base.GetDefinitionQuery();
        this._tempBlockQuery = base.GetEntityQuery(new ComponentType[]
        {
                ComponentType.ReadOnly<Temp>(),
                ComponentType.ReadOnly<Block>(),
                ComponentType.ReadWrite<Cell>()
        });
        this._soundQuery = base.GetEntityQuery(new ComponentType[]
        {
                ComponentType.ReadOnly<ToolUXSoundSettingsData>()
        });
        this._applyAction = InputManager.instance.FindAction("Tool", "Apply");
        this._secondaryApplyAction = InputManager.instance.FindAction("Tool", "Secondary Apply");
        this._snapPoint = new NativeValue<ControlPoint>(Allocator.Persistent);
        this.overwrite = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this._snapPoint.Dispose();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.requireZones = true;
        this.requireAreas = Game.Areas.AreaTypeMask.Lots;
        this._raycastPoint = default;
        this._startPoint = default;
        this._state = State.Default;
        this._applyAction.shouldBeEnabled = true;
        this._applyAction.SetDisplayProperties("Apply Zone", 20);
        this._secondaryApplyAction.shouldBeEnabled = true;
        this._secondaryApplyAction.SetDisplayProperties("Remove Zone", 25);
    }

    protected override void OnStopRunning()
    {
        this._applyAction.shouldBeEnabled = false;
        this._secondaryApplyAction.shouldBeEnabled = false;
        base.OnStopRunning();        
    }

    public override PrefabBase GetPrefab()
    {
        
        return this.prefab;
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        return false;
    }

    public override void InitializeRaycast()
    {
        base.InitializeRaycast();
        if (this.prefab == null)
        {
            this.m_ToolRaycastSystem.typeMask = Game.Common.TypeMask.None;
            return;
        }

        Snap onMask;
        Snap offMask;
        this.GetAvailableSnapMask(out onMask, out offMask);
        // Usually switch statement for mode, but just doing Marquee to test
        if ((ToolBaseSystem.GetActualSnap(this.selectedSnap, onMask, offMask) & Snap.ExistingGeometry) != Snap.None)
        {
            this.m_ToolRaycastSystem.typeMask = (TypeMask.Terrain | TypeMask.Zones);
            return;
        }
        this.m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (this.m_FocusChanged)
        {
            return inputDeps;
        }

        if(this._state != State.Default && (!this._applyAction.enabled | !this._secondaryApplyAction.enabled))
        {
            this._startPoint = default;
            this._state = State.Default;
            return this.Clear(inputDeps);
        }

        if (this.prefab != null)
        {
            base.UpdateInfoview(this.m_PrefabSystem.GetEntity(this.prefab));
            this.GetAvailableSnapMask(out this.m_SnapOnMask, out this.m_SnapOffMask);
            if ((this.m_ToolRaycastSystem.raycastFlags & (RaycastFlags.DebugDisable | RaycastFlags.UIDisable)) == (RaycastFlags)0U) {
                if (this._state != State.Default)
                {
                    if (this._applyAction.WasPressedThisFrame() || this._applyAction.WasReleasedThisFrame())
                    {
                        return this.Apply(inputDeps, false);
                    }
                    if (this._secondaryApplyAction.WasPressedThisFrame() || this._secondaryApplyAction.WasReleasedThisFrame())
                    {
                        return this.Cancel(inputDeps, false);
                    }
                    return this.Update(inputDeps);
                } else
                {
                    if (this._secondaryApplyAction.WasPressedThisFrame())
                    {
                        return this.Cancel(inputDeps, this._secondaryApplyAction.WasReleasedThisFrame());
                    }
                    if (this._applyAction.WasPressedThisFrame())
                    {
                        return this.Apply(inputDeps, this._applyAction.WasReleasedThisFrame());
                    }
                    return this.Update(inputDeps);
                }
            } else
            {
                base.UpdateInfoview(Entity.Null);
            }

            if (this._state != State.Default && (this._applyAction.WasReleasedThisFrame() || this._secondaryApplyAction.WasReleasedThisFrame()))
            {
                this._startPoint = default(ControlPoint);
                this._state = State.Default;
            }
            return this.Clear(inputDeps);
        }

        return base.OnUpdate(inputDeps);
    }

    private JobHandle Update(JobHandle inputDeps)
    {
        throw new NotImplementedException();
    }

    private JobHandle Cancel(JobHandle inputDeps, bool singleFrameOnly)
    {
        if (this._state == State.Default)
        {
            //marquee mode only rn
            this._audioManager.PlayUISound(this._soundQuery.GetSingleton<ToolUXSoundSettingsData>().m_ZoningMarqueeClearStartSound, 1f);
            base.applyMode = ApplyMode.Clear;
            if (!singleFrameOnly)
            {
                this._startPoint = this._snapPoint.value;
                this._state = State.Dezoning;
            }
            this.GetRaycastResult(out this._raycastPoint);
            JobHandle jobHandle = this.SnapPoint(inputDeps);
            JobHandle job = this.SetZoneType(jobHandle);
            JobHandle job2 = this.UpdateDefinitions(jobHandle);
            return JobHandle.CombineDependencies(job, job2);
        }
        //TODO: finish implementing
        return inputDeps;
    }

    private JobHandle SnapPoint(JobHandle inputDeps)
    {
        throw new NotImplementedException();
    }

    private JobHandle UpdateDefinitions(JobHandle jobHandle)
    {
        throw new NotImplementedException();
    }

    private JobHandle SetZoneType(JobHandle jobHandle)
    {
        throw new NotImplementedException();
    }

    private JobHandle Apply(JobHandle inputDeps, bool v)
    {
        throw new NotImplementedException();
    }

    private JobHandle Clear(JobHandle inputDeps)
    {
        base.applyMode = ApplyMode.Clear;
        return inputDeps;
    }
}
