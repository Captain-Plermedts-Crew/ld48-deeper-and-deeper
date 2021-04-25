// using UnityEngine;

// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using static Unity.Mathematics.math;
// using Unity.Physics;
// using Unity.Physics.Systems;

// [UpdateInGroup(typeof(Unity.Entities.FixedStepSimulationSystemGroup))]
// [UpdateAfter(typeof(StepPhysicsWorld))]
// [UpdateBefore(typeof(EndFramePhysicsSystem))]
// public class CollisionSystem : JobComponentSystem
// {
//     private BuildPhysicsWorld buildPhysicsWorld;
//     private StepPhysicsWorld stepPhysicsWorld;

//     private EndSimulationEntityCommandBufferSystem commandBufferSystem;

//     protected override void OnCreate()
//     {
//         base.OnCreate();
//         buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
//         stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
//         commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//     }

//     // [BurstCompile]
//     struct PickupOnTriggerSystemJob : ICollisionEventsJob
//     {
//         [ReadOnly] public ComponentDataFromEntity<Temperature> allTemperatures;
//         // [ReadOnly] public ComponentDataFromEntity<PlayerTag> allPlayers;

//         public EntityCommandBuffer entityCommandBuffer;

//         public void Execute(CollisionEvent triggerEvent)
//         {
//             Entity entityA = triggerEvent.EntityA;
//             Entity entityB = triggerEvent.EntityB;
        
//             if (allTemperatures.HasComponent(entityA) && allTemperatures.HasComponent(entityB))
//             {
//                 Temperature aT = allTemperatures[entityA];
//                 Temperature bT = allTemperatures[entityB];
//                 Debug.Log("before  " + bT.temperature);

//                 float diff = aT.temperature - bT.temperature;
//                 aT.temperature -= diff/10;
//                 bT.temperature += diff/10;
//                 Debug.Log("after  " + bT.temperature);
//             }
//         }
//     }

//     protected override JobHandle OnUpdate(JobHandle inputDependencies)
//     {
//         var job = new PickupOnTriggerSystemJob();
//         job.allTemperatures = GetComponentDataFromEntity<Temperature>(true);
//         job.entityCommandBuffer = commandBufferSystem.CreateCommandBuffer();

//         JobHandle jobHandle = job.Schedule(
//             stepPhysicsWorld.Simulation, 
//             ref buildPhysicsWorld.PhysicsWorld,
//             inputDependencies);


//         jobHandle.Complete();
//         commandBufferSystem.AddJobHandleForProducer(jobHandle);
//         return jobHandle;
//     }
// }