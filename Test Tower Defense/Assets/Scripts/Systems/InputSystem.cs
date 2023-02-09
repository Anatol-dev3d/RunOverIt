using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using static UnityEngine.EventSystems.EventTrigger;
using Debug = UnityEngine.Debug;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct InputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        DynamicBuffer<Towers> towers = SystemAPI.GetSingletonBuffer<Towers>();
        var ecbBOS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var input in SystemAPI.Query<DynamicBuffer<TowerPlacementInput>>())
        {
            foreach (var placementInput in input)
            {
                if (physicsWorld.CastRay(placementInput.Value, out var hit))
                {
                    Debug.Log($"{hit.Position}");
                    var towerPosition = math.round(hit.Position) + math.up();
                    NativeList<DistanceHit> distances = new NativeList<DistanceHit>(Allocator.Temp);
                    if (!physicsWorld.OverlapSphere(towerPosition + math.up(), 0.1f, ref distances, CollisionFilter.Default))
                    {
                        var entity = ecbBOS.Instantiate(towers[placementInput.Index].Prefab);
                        ecbBOS.SetComponent(entity, LocalTransform.FromPosition(towerPosition));
                    }               
                }
            }
            input.Clear();
        }
    }
}