using UnityEngine;
using Unity.Entities;

public class TemperatureSystem : SystemBase {
    private static float freezeTemp = 0;
    private static float igniteTemp = 10;

    public EndSimulationEntityCommandBufferSystem ECBS;

    protected override void OnCreate() {
        base.OnCreate();
 
        //Populate the reference
        ECBS = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    // runs every frame
    protected override void OnUpdate()
    {

        EntityCommandBuffer ecb = ECBS.CreateCommandBuffer();

        Entities
        .WithoutBurst()
        .WithAll<Temperature>()
        .ForEach((Entity entity, ref Temperature tempComponent) =>
        {
            // decrement by time elapsed for one frame
            tempComponent.temperature -= tempComponent.tempLossRate * Time.DeltaTime;   

            
            if (tempComponent.temperature < freezeTemp){
                if (!HasComponent<FrozenTag>(entity)){
                    ecb.AddComponent(entity, new FrozenTag{});
                }
                if (HasComponent<IgnitedTag>(entity)){
                    ecb.RemoveComponent<IgnitedTag>(entity);
                }                
            } else if (tempComponent.temperature >= freezeTemp && tempComponent.temperature <= igniteTemp){
                if (HasComponent<FrozenTag>(entity)){
                    ecb.RemoveComponent<FrozenTag>(entity);
                }
                if (HasComponent<IgnitedTag>(entity)){
                    ecb.RemoveComponent<IgnitedTag>(entity);
                }  
            } else {
                if (HasComponent<FrozenTag>(entity)){
                    ecb.RemoveComponent<FrozenTag>(entity);
                }
                if (!HasComponent<IgnitedTag>(entity)){
                    ecb.AddComponent(entity, new IgnitedTag{});
                } 
            }

        }).Run();
        ECBS.AddJobHandleForProducer(Dependency);
    }

}

// public class TemperatureSystem : JobComponentSystem {

//     private EndSimulationEntityCommandBufferSystem commandBufferSystem;

//     protected override void OnCreate(){
//         base.OnCreate();
//         commandBufferSystem = World
//             .DefaultGameObjectInjectionWorld
//             .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
//     }

//      protected override JobHandle OnUpdate(JobHandle inputDeps){

//         var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();   
//         float dt = Time.DeltaTime;

//         var jobHandle = Entities
//             .WithAll<Temperature>()
//             .ForEach((Entity entity, int entityInQueryIndex, ref Temperature tempComponent) => {
//               tempComponent.temperature -= tempComponent.tempLossRate * dt;   

//               if (tempComponent.temperature < 0){
//                   commandBuffer.AddComponent<FrozenTag>(entityInQueryIndex, entity);
//               } 
//             })
//             .Schedule(inputDeps);
//         jobHandle.Complete();
//         return jobHandle;
//     }   
// }
