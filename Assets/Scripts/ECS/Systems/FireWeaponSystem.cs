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
            .WithAll<WeaponComponent, UserInputData>()
            .ForEach((ref WeaponComponent weapon, ref UserInputData input, ref LocalToWorld localToWorld) => {
                
                weapon.shotTimer += deltaTime;
                
                if(input.IsFiring &&
                    weapon.shotTimer >= weapon.rateOfFire){

                    weapon.shotTimer = 0f;

                    Debug.Log(weapon.shotTimer);

                    float3 position = math.transform(localToWorld.Value, new float3(0, 0, 0));

                    var entity = commandBuffer.Instantiate(bulletEntityPrefab);
                    commandBuffer.SetComponent(entity, new Translation { Value = position });
                    commandBuffer.SetComponent(entity, new Rotation { Value = localToWorld.Rotation });
                }

            })
            .Run();
    }
}