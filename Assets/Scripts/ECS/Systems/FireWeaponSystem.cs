using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;


[UpdateAfter(typeof(GatherInputSystem))]
[UpdateAfter(typeof(RotatePlayerToMouseSystem))]
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
                weapon.shotTimer = 1000; // bypasses the shotTimer for a more flameThrower aesthetic; can fine-tune particle heat in the Temp system

                if(input.IsFiring &&
                    weapon.shotTimer >= weapon.rateOfFire){

                    weapon.shotTimer = 0f;

                    float3 position = math.transform(localToWorld.Value, new float3(0, 0, 0));

                    //we need to use this fucking thing to get the ACTUAL rotation, because the torches scale can mess with localToWorld quats
                    var rot = Unity.Physics.Math.DecomposeRigidBodyOrientation(localToWorld.Value);

                    var entity = commandBuffer.Instantiate(bulletEntityPrefab);
                    commandBuffer.SetComponent(entity, new Translation { Value = position });
                    commandBuffer.SetComponent(entity, new Rotation { Value = rot });
                    commandBuffer.SetComponent(entity, new Temperature { Value = TemperatureSystem.igniteTemp * 3f });
                    GameManager.Instance.PlayerManager.PlayWeaponFX();
                }

            })
            .Run();
    }
}