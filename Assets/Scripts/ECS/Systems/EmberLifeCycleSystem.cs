using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

using Unity.Physics;
using Collider = Unity.Physics.Collider;
using MeshCollider = Unity.Physics.MeshCollider;
using PhysicsMaterial = Unity.Physics.Material;

using System;
using Random = Unity.Mathematics.Random;

public class EmberLifeCycleSystem : SystemBase {

    private Mesh sphereMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().mesh;//((GameObject)Resources.Load ("Models/SpaceFighterAlt")).GetComponent<MeshFilter>().sharedMesh;
    private UnityEngine.Material fireMaterial = Resources.Load("FireMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material;

    private EndSimulationEntityCommandBufferSystem ECBS;
    private EntityArchetype emberArchetype;

    private static Random r = new Random(1241252);


    protected override void OnCreate() {
        base.OnCreate();
        //Populate the reference
        ECBS = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        emberArchetype = getEmberEntityArchetype(entityManager);
    }


    // runs every frame
    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = ECBS.CreateCommandBuffer();

        float deltaTime = Time.DeltaTime;
        float igniteTemp = TemperatureSystem.igniteTemp;

        //Create new ember loop
        Entities
            .WithStructuralChanges()
            .WithoutBurst()
            .WithAll<IgnitedTag>()
            .WithAll<Translation>()
            .WithAll<Temperature>()
            .WithNone<EmberTag>()
            .ForEach((Entity entity, in Translation translation, in Temperature temperature) => 
            {
        
                float roll = r.NextFloat(0, 1f);// + .01f*temperature.Value/igniteTemp;//chance in 0 to temperature.Value

                if (roll > .99f){
                    float3 newPos = new float3(translation.Value);
                    newPos = new float3((float)newPos.x+(float)r.NextFloat(-.5f,.5f),
                        (float)newPos.y+(float)r.NextFloat(.5f,.6f), 
                        (float)newPos.z+(float)r.NextFloat(-.5f,.5f));
                    // newPos.y=newPos.y+.5f;
                    // newPos.x=newPos.z+

                    EmberLifeCycleSystem.createEmber(
                        ecb,
                        emberArchetype,
                        newPos,
                        Quaternion.Euler(0, 0, 0),
                        sphereMesh,
                        fireMaterial,
                        new float3(r.NextFloat(0,.5f), r.NextFloat(0,6f), r.NextFloat(0,.5f))
                        );
                }
                


            })
            .Run();

        //Destroy if temp is not high enough; also reduce temp of ember tag
        Entities
            .WithStructuralChanges()
            .WithoutBurst()
            .WithAll<EmberTag>()
            .WithAll<Temperature>()
            .ForEach((Entity entity, ref Temperature temperature) => {
                temperature.Value -= temperature.Rate*deltaTime;
                if (temperature.Value < TemperatureSystem.igniteTemp){
                    ecb.DestroyEntity(entity);
                }
            })
            .Run();

        ECBS.AddJobHandleForProducer(Dependency);
    }

    private static unsafe Entity createEmber(
        EntityCommandBuffer ecb,
        EntityArchetype archetype,
        float3 position, 
        quaternion orientation,
        Mesh mesh,
        UnityEngine.Material material,
        float3 velocity){


        Entity entity = ecb.CreateEntity(archetype);
        
        //Transform stuff
        ecb.SetComponent(entity, new Translation { Value = position });
        ecb.SetComponent(entity, new Rotation { Value = orientation });
        ecb.SetComponent(entity, new Scale { Value = .0625f });
        // ecb.SetComponent(entity, new Scale { Value = .5f });

        //custom stuff
        int igniteTemp = (int)TemperatureSystem.igniteTemp;
        ecb.SetComponent(entity, new Temperature{ Value = r.NextInt(igniteTemp*2,igniteTemp*5), Rate = igniteTemp });   

        //RenderMesh & Bounds   
        ecb.SetSharedComponent(entity, new RenderMesh 
		{
			mesh = mesh,
			material = material
		});
        ecb.SetComponent(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

        Unity.Physics.Material physicsMaterial = new Unity.Physics.Material{
            CollisionResponse = Unity.Physics.CollisionResponsePolicy.None
        };
        

        BlobAssetReference<Collider> collider = Unity.Physics.SphereCollider
            .Create(new SphereGeometry {
                Center = float3.zero,
                Radius = .01f,
            },
            CollisionFilter.Zero,
            physicsMaterial
        );

        ecb.SetComponent(entity, new PhysicsCollider { Value = collider });
        ecb.SetComponent(entity, new PhysicsGravityFactor { Value = .1f });

        float mass = 5f;
        Collider* colliderPtr = (Collider*)collider.GetUnsafePtr();
        ecb.SetComponent(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));
        // Calculate the angular velocity in local space from rotation and world angular velocity
        float3 angularVelocityLocal = math.mul(math.inverse(colliderPtr->MassProperties.MassDistribution.Transform.rot), float3.zero);
        ecb.SetComponent(entity, new PhysicsVelocity()
        {
            Linear = velocity,
            Angular = 0
        });
        ecb.SetComponent(entity, new PhysicsDamping()
        {
            Linear = .5f,
            Angular = 10f
        });

        return entity;

    }

    public static EntityArchetype getEmberEntityArchetype(EntityManager entityManager){
        ComponentType[] renderedPhysicsComponents = EntityUtils.getRenderedPhysicsComponents(true);
        ComponentType[] specificComponents = new ComponentType[]{
            typeof(IgnitedTag),
            typeof(EmberTag),
            typeof(Temperature),
            typeof(Scale)};
        return entityManager.CreateArchetype(EntityUtils.mergeComponentTypes(specificComponents, renderedPhysicsComponents));
    }   
}
