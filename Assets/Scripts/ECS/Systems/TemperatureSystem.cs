using UnityEngine;
using Unity.Entities;

using Unity.Mathematics;
using Unity.Transforms;

public class TemperatureSystem : SystemBase {
    public static float freezeTemp = 0;
    public static float igniteTemp = 10;
    
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
            .ForEach((Entity entity, ref Temperature temperature) =>
            {
                // decrement by time elapsed for one frame
                // temperature.Value -= temperature.tempLossRate * Time.DeltaTime;   
                
                //All for Safety in case something goes wrong
                if (temperature.Value < freezeTemp){ //we're freezing
                    if (!HasComponent<FrozenTag>(entity)){
                        ecb.AddComponent(entity, new FrozenTag{});
                    }
                    if (HasComponent<IgnitedTag>(entity)){
                        ecb.RemoveComponent<IgnitedTag>(entity);
                    }                
                } else if (temperature.Value >= freezeTemp && temperature.Value <= igniteTemp){ //we're good
                    if (HasComponent<FrozenTag>(entity)){
                        ecb.RemoveComponent<FrozenTag>(entity);
                    }
                    if (HasComponent<IgnitedTag>(entity)){
                        ecb.RemoveComponent<IgnitedTag>(entity);
                    }  
                } else if (temperature.Value > igniteTemp){ //we're hot
                    if (HasComponent<FrozenTag>(entity)){
                        ecb.RemoveComponent<FrozenTag>(entity);
                    }
                    if (!HasComponent<IgnitedTag>(entity)){
                        ecb.AddComponent(entity, new IgnitedTag{});
                    }
                }

            })
            .Run();

        //end game if player has a frozen tag
        Entities
            .WithoutBurst()
            .WithAll<PlayerTag, Temperature>()
            .ForEach((Entity entity, in Temperature temp, in PlayerTag player) => {
                GameManager.UpdateTemperatue(temp.Value);
                if (HasComponent<FrozenTag>(entity)){
                    GameManager.EndGame();
                }
            })
            .Run();

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
