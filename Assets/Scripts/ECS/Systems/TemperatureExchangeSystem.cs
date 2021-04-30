using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;



public class TemperatureExchangeSystem : SystemBase {

    private struct EntityWithTemperature {
        public Entity entity;
        public float3 position;
        public float temperature;
    }
    
    [RequireComponentTag(typeof(Translation), typeof(Temperature))]
    [BurstCompile]
    private struct FindTargetBurstJob : IJobEntityBatch {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithTemperature> targetArray;
        
        public float deltaTime;
        [ReadOnly] public ComponentTypeHandle<Translation> TranslationType;
        public ComponentTypeHandle<Temperature> TemperatureType;
        // public NativeArray<Entity> closestTargetEntityArray;

        public void Execute(ArchetypeChunk chunk, int chunkIndex ){
            var chunkTranslations = chunk.GetNativeArray(TranslationType);
            var chunkTemperatures = chunk.GetNativeArray(TemperatureType);

            //iterate through each entity in this chunk
            for (int i=0; i<chunk.Count; i++){
                float tempDiffSum = 0;
                // float tempSum = 0;
                int numCloseEntities = 0;
                Translation translation = chunkTranslations[i];
                Temperature temperature = chunkTemperatures[i];
                
                //iterate through all other entities
                for (int j=0; j<targetArray.Length; j++) {
                    // Cycling through all target entities
                    EntityWithTemperature targetEntityWithTemperature = targetArray[j];

                    float distanceToOtherEntity = math.distance(translation.Value, targetEntityWithTemperature.position);
                    if (distanceToOtherEntity < 3){
                        // tempSum+=targetEntityWithTemperature.temperature;
                        numCloseEntities++;
                        // Debug.Log("distance " + distanceToOtherEntity);

                        float temperatureDiff = temperature.Value - targetEntityWithTemperature.temperature;
                        tempDiffSum += temperatureDiff*deltaTime;
                    }
                    
                    
                }
                // Debug.Log("diff: " + tempDiffSum + " before: " + temperature.Value + " after: " + (temperature.Value-tempDiffSum));
                chunkTemperatures[i] = new Temperature{
                    Value = temperature.Value - (tempDiffSum/numCloseEntities)*deltaTime,
                    Rate = temperature.Rate
                };
                // Mathf.min(tempSum / numCloseEntities);
                // float meanTemp = tempSum / numCloseEntities;
                // Debug.Log("temp: " + temperature.Value + " mean temp: " + meanTemp + "numCloseEntities " + numCloseEntities + "new temp" + ((temperature.Value + meanTemp) / 2f) );
                // chunkTemperatures[i] = new Temperature{
                //     Value =(temperature.Value + meanTemp) / 2f,
                //     Rate = temperature.Rate
                // };
            }
            
        
        }

    }

    EntityQuery targetQuery;

    protected override void OnCreate(){
        targetQuery = GetEntityQuery(typeof(Temperature), ComponentType.ReadOnly<Translation>());
    }

    // protected override JobHandle OnUpdate(JobHandle inputDeps) {    
    protected override void OnUpdate(){
        //set the query for temp and translation
        float deltaTime =  Time.DeltaTime;

        //convert to native arrays of entities and components
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Temperature> targetTemperatureArray = targetQuery.ToComponentDataArray<Temperature>(Allocator.TempJob);

        //allocate a EntityWithTemperature struct array
        NativeArray<EntityWithTemperature> targetArray = new NativeArray<EntityWithTemperature>(targetEntityArray.Length, Allocator.TempJob);

        //fill struct array
        for (int i = 0; i < targetEntityArray.Length; i++) {
            targetArray[i] = new EntityWithTemperature {
                entity = targetEntityArray[i],
                position = targetTranslationArray[i].Value,
                temperature = targetTemperatureArray[i].Value
            };
        }

        //clean up
        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();
        targetTemperatureArray.Dispose();

        //do work son
        FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob {
            targetArray = targetArray,
            deltaTime = deltaTime,
            TranslationType = GetComponentTypeHandle<Translation>(true),
            TemperatureType = GetComponentTypeHandle<Temperature>(false)
        };

        findTargetBurstJob.Run(targetQuery); //runs as one big chunk (faster apparently, unless the function we're doing is particularly complicated)
        // Dependency = findTargetBurstJob.ScheduleParallel(targetQuery, 1, Dependency); //runs in parallel
        // targetEntityArray.Dispose();

    }
}