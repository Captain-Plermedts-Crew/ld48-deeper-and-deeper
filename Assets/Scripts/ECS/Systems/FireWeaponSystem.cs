using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;


[UpdateAfter(typeof(GatherInputSystem))]
public class FireWeaponSystem : SystemBase
{
    private EntityManager entityManager;
    private Entity bulletEntityPrefab;

    private float shotTimer;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;
    EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    protected override void OnCreate()
    {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        bulletEntityPrefab = EntityManager.CreateEntityQuery(typeof(BulletPrefab))
            .GetSingleton<BulletPrefab>().Value;
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var commandBuffer = barrier.CreateCommandBuffer();

        Entities
            .WithoutBurst()
            .WithAll<PlayerTag, UserInputData>()
            .ForEach((in Translation tran, in Rotation rot, in UserInputData input) => {
                if(input.IsFiring){
                    Debug.Log("FIRE!");
                    var entity = commandBuffer.Instantiate(bulletEntityPrefab);
                    commandBuffer.SetComponent(entity, new Translation { Value = tran.Value });
                    commandBuffer.SetComponent(entity, new Rotation { Value = rot.Value });
                }

            })
            .Run();
    }
 
    // private struct SpawnBulletJob : IJobForEachWithEntity<Translation, Rotation, UserInputData>
    // {
    //     private EntityCommandBuffer.Concurrent buffer;
    //     private readonly float _deltaTime;

    //     public SpawnBulletJob(EntiyCommandbuffer buffer, float deltaTime){
    //         _deltaTime = deltaTime;
    //     }

    //     public void Execute(Entity entity, 
    //         int index,
    //         in Translation trans,
    //         in Rotation rot,
    //         in UserInputData input
    //     {

    //     }
    // }
}