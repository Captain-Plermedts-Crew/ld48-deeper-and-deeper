using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;

public class TemperatureSystem : JobComponentSystem {

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate(){
        base.OnCreate();
        commandBufferSystem = World
            .DefaultGameObjectInjectionWorld
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
    }

     protected override JobHandle OnUpdate(JobHandle inputDeps){

        var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();   
        float dt = Time.DeltaTime;

        var jobHandle = Entities
            .WithAll<Temperature>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Temperature tempComponent) => {
              tempComponent.temperature -= tempComponent.tempLossRate * dt;    
            })
            .Schedule(inputDeps);
        jobHandle.Complete();
        return jobHandle;
    }   
}
