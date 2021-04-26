using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;



public class FindTargetJobSystem : SystemBase {

    private struct EntityWithPosition {
        public Entity entity;
        public float3 position;
        public float temperature;
    }

    // [RequireComponentTag(typeof(Unit))]
    // [ExcludeComponent(typeof(HasTarget))]
    // [BurstCompile]
    // private struct FindTargetJob : IJobForEachWithEntity<Translation> {

    //     [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
    //     public EntityCommandBuffer.Concurrent entityCommandBuffer;

    //     public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation) {
    //         float3 unitPosition = translation.Value;
    //         Entity closestTargetEntity = Entity.Null;
    //         float3 closestTargetPosition = float3.zero;

    //         for (int i=0; i<targetArray.Length; i++) {
    //             // Cycling through all target entities
    //             EntityWithPosition targetEntityWithPosition = targetArray[i];

    //             if (closestTargetEntity == Entity.Null) {
    //                 // No target
    //                 closestTargetEntity = targetEntityWithPosition.entity;
    //                 closestTargetPosition = targetEntityWithPosition.position;
    //             } else {
    //                 if (math.distance(unitPosition, targetEntityWithPosition.position) < math.distance(unitPosition, closestTargetPosition)) {
    //                     // This target is closer
    //                     closestTargetEntity = targetEntityWithPosition.entity;
    //                     closestTargetPosition = targetEntityWithPosition.position;
    //                 }
    //             }
    //         }

    //         // Closest Target
    //         if (closestTargetEntity != Entity.Null) {
    //             entityCommandBuffer.AddComponent(index, entity, new HasTarget { targetEntity = closestTargetEntity });
    //         }
    //     }

    // }
    
    [RequireComponentTag(typeof(Translation), typeof(Temperature))]
    [BurstCompile]
    private struct FindTargetBurstJob : IJobEntityBatch {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        
        public float deltaTime;
        [ReadOnly] public ComponentTypeHandle<Translation> TranslationType;
        public ComponentTypeHandle<Temperature> TemperatureType;
        // public NativeArray<Entity> closestTargetEntityArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex ){//[ReadOnly] ref Translation translation, ref Temperature temperature) {
            var chunkTranslations = chunk.GetNativeArray(TranslationType);
            var chunkTemperatures = chunk.GetNativeArray(TemperatureType);
            

            
            //iterate through each entity in this chunk
            for (int i=0; i<chunk.Count; i++){
                float tempDiffSum = 0;
                Translation translation = chunkTranslations[i];
                Temperature temperature = chunkTemperatures[i];
                
                //iterate through all other entities
                for (int j=0; j<targetArray.Length; j++) {
                    // Cycling through all target entities
                    EntityWithPosition targetEntityWithPosition = targetArray[j];

                    float distanceToOtherEntity = math.distance(translation.Value, targetEntityWithPosition.position);
                    if (distanceToOtherEntity < 3 && distanceToOtherEntity !=0){
                        // Debug.Log("distance " + distanceToOtherEntity);

                        float temperatureDiff = temperature.Value - targetEntityWithPosition.temperature;
                        tempDiffSum += temperatureDiff*deltaTime;
                    }
                    
                    
                }
                // Debug.Log("diff: " + tempDiffSum + " before: " + temperature.Value + " after: " + (temperature.Value-tempDiffSum));
                chunkTemperatures[i] = new Temperature{
                    Value = temperature.Value - tempDiffSum,
                    Rate = temperature.Rate
                };
            }
            
            
            // float3 unitPosition = translation.Value;
            // float unitTemperature = temperature.Value;
            // // Entity closestTargetEntity = Entity.Null;
            // // float3 closestTargetPosition = float3.zero;
            // // float3 closestTargetTemperature = 0;

            // float tempDiffSum = 0;
            // for (int i=0; i<targetArray.Length; i++) {
            //     // Cycling through all target entities
            //     EntityWithPosition targetEntityWithPosition = targetArray[i];

            //     float distanceToOtherEntity = math.distance(unitPosition, targetEntityWithPosition.position);
            //     float temperatureDiff = unitTemperature - targetEntityWithPosition.temperature;
            //     tempDiffSum += temperatureDiff;

            //     if (closestTargetEntity == Entity.Null) {
            //         // No target
            //         closestTargetEntity = targetEntityWithPosition.entity;
            //         closestTargetPosition = targetEntityWithPosition.position;
            //     } else {
            //         if (math.distance(unitPosition, targetEntityWithPosition.position) < math.distance(unitPosition, closestTargetPosition)) {
            //             // This target is closer
            //             closestTargetEntity = targetEntityWithPosition.entity;
            //             closestTargetPosition = targetEntityWithPosition.position;
            //             closestTargetTemperature = targetEntityWithPosition.temperature;
            //         }
            //     }
            // }
            // temperature.Value += tempDiffSum * .01;

            // closestTargetEntityArray[index] = closestTargetEntity;
        }

    }
    
    // [RequireComponentTag(typeof(Unit))]
    // [ExcludeComponent(typeof(HasTarget))]
    // private struct AddComponentJob : IJobForEachWithEntity<Translation> {

    //     [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> closestTargetEntityArray;
    //     public EntityCommandBuffer.Concurrent entityCommandBuffer;

    //     public void Execute(Entity entity, int index, ref Translation translation) {
    //         if (closestTargetEntityArray[index] != Entity.Null) {
    //             entityCommandBuffer.AddComponent(index, entity, new HasTarget { targetEntity = closestTargetEntityArray[index] });
    //         }
    //     }

    // }
    EntityQuery targetQuery;

    protected override void OnCreate(){
        targetQuery = GetEntityQuery(typeof(Temperature), ComponentType.ReadOnly<Translation>());
    }

    // protected override JobHandle OnUpdate(JobHandle inputDeps) {    
    protected override void OnUpdate(){
        //set the query for temp and translation
        // EntityQuery targetQuery = GetEntityQuery(typeof(Temperature), ComponentType.ReadOnly<Translation>());

        //convert to native arrays of entities and components
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Temperature> targetTemperatureArray = targetQuery.ToComponentDataArray<Temperature>(Allocator.TempJob);

        //allocate a EntityWithPosition struct array
        NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetEntityArray.Length, Allocator.TempJob);

        //fill struct array
        for (int i = 0; i < targetEntityArray.Length; i++) {
            targetArray[i] = new EntityWithPosition {
                entity = targetEntityArray[i],
                position = targetTranslationArray[i].Value,
                temperature = targetTemperatureArray[i].Value
            };
        }

        //clean up
        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();
        targetTemperatureArray.Dispose();
        
        // EntityQuery unitQuery = GetEntityQuery(typeof(Unit), ComponentType.Exclude<HasTarget>());
        // NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateLength(), Allocator.TempJob);
       
        //do work son
        FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob {
            targetArray = targetArray,
            deltaTime = .01f,
            TranslationType = GetComponentTypeHandle<Translation>(true),
            TemperatureType = GetComponentTypeHandle<Temperature>(false)
            // closestTargetEntityArray = closestTargetEntityArray
        };
        // targetArray.Dispose();

        // state.Dependency = findTargetBurstJob.ScheduleSingle(targetQuery, state.Dependency);
        // JobHandle jobHandle = findTargetBurstJob.Schedule(this, inputDeps);
        Dependency = findTargetBurstJob.ScheduleParallel(targetQuery, 1, Dependency);
        // AddComponentJob addComponentJob = new AddComponentJob {
        //     closestTargetEntityArray = closestTargetEntityArray,
        //     entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
        // };
        // jobHandle = addComponentJob.Schedule(this, jobHandle);
        
        // endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        // return jobHandle;
    }
    

}


// public class DetectByDistanceSystem : SystemBase
//     {
//         protected EntityQuery query;
 
//         protected override void OnCreate()
//         {
//             base.OnCreate();
 
//             query = GetEntityQuery(ComponentType.ReadOnly<Temperature>(),
//                                    ComponentType.ReadOnly<Translation>());
//         }
 
//         protected override void OnUpdate()
//         {
//             var detectableEntities = query.ToEntityArray(Allocator.TempJob);
//             var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
//             var temperatures = query.ToComponentDataArray<Temperature>(Allocator.TempJob);
 
//             var deltaTime = Time.DeltaTime;
 
//             Entities
//                 // .WithReadOnly(detectableEntities)
//                 // .WithReadOnly(translations)
//                 // .WithDeallocateOnJobCompletion(detectableEntities)
//                 // .WithDeallocateOnJobCompletion(translations)
//                 // .WithDeallocateOnJobCompletion(temperatures)
//                 .ForEach((Entity entity,
//                            ref Temperature temperature,
//                            in Translation translation) => {
                
//                     int otherIndex;
    
//                     // float shortestDistanceSq = float.MaxValue;
    
//                     for (otherIndex = 0; otherIndex < detectableEntities.Length; otherIndex++)
//                     {
//                         if (detectableEntities[otherIndex] == entity)
//                         {
//                             continue;
//                         }
    
//                         float distSq = math.distancesq(translation.Value, translations[otherIndex].Value);

//                         if (distSq<3){
//                             float diff = temperature.temperature - temperatures[otherIndex].temperature;
//                                 temperature.temperature  -= diff * .01f;
//                                 temperatures[otherIndex].temperature += diff * .01f;
//                         }
    
//                         // if (distSq > shortestDistanceSq)
//                         // {
//                         //      continue;
//                         // }
    
//                         // detectedEntity.value = detectableEntities[otherIndex];
//                         // shortestDistanceSq = distSq; 
//                     }
    
//                 })
//                 .ScheduleParallel();
//         }
//     }


// public class DestroyAfterLifespan : SystemBase {

//     private EndSimulationEntityCommandBufferSystem commandBufferSystem;

//     protected override void OnCreate(){
//         base.OnCreate();
//         commandBufferSystem = World
//             .DefaultGameObjectInjectionWorld
//             .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
//     }

//      protected override void OnUpdate(){

//         var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();   
//         float dt = Time.DeltaTime;

//         Entities
//             .WithAll<Temperature>()
//             .ForEach((Entity e0, ref Translation pos0, ref Temperature tc0) => {
//                 float3 pos0Value = pos0.Value;
//                 float tc0Value = tc0.temperature;

//                 Entities
//                     .WithAll<Temperature>()
//                     .ForEach((Entity e1, ref Translation pos1, ref Temperature tc1) => {
//                         if (math.distance(pos0Value, pos1.Value) <= 5)
//                         {
//                             // PostUpdateCommands.DestroyEntity(enemy);                        
//                             Debug.Log("before  " + tc1.temperature);

//                             float diff = tc0Value - tc1.temperature;
//                             tc0Value -= diff * dt;
//                             tc1.temperature += diff * dt;
//                             Debug.Log("after  " + tc1.temperature);
                            
//                         }
//                     })
//                     .Run();
//             })
//             .Run();

//     }   
// }