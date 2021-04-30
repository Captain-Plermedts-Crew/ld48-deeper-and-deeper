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

    private float emberCountdown = .3f;
    public static float maxEmberCountdown = 0.1f;

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
        float deltaTime =  Time.DeltaTime;


        emberCountdown -= deltaTime;
        //Check each entity for a chance to create an ember
        //only create embers once the cooldown is reached
        if (emberCountdown<=0){
            emberCountdown = maxEmberCountdown; //reset cooldown

            Entities
                .WithStructuralChanges()
                .WithoutBurst()
                .WithAll<IgnitedTag>()
                .WithAll<Translation>()
                .WithAll<Temperature>()
                .WithNone<EmberTag>()
                .ForEach((Entity entity, in Translation translation, in Temperature temperature) => 
                {
                    float normalizedTemp = Mathf.Min(temperature.Value / TemperatureSystem.igniteTemp, 5f); //with a a max of 5, the max chance is 0.6224594;
                    float maxChanceOfEmber = sigmoid(.1f, normalizedTemp);//.1f flattens out the sigmoid
                    int numEmbers = ((int) normalizedTemp+1);  //hotter things create more embers

                    //roll for each ember separately
                    for (int i=0; i < numEmbers; i++){                        
                        float chanceOfEmber = r.NextFloat(0f, maxChanceOfEmber); //roll from 0 to maxChanceOfEmber

                        // Debug.Log(normalizedTemp + " " + maxChanceOfEmber + " " + ((int)normalizedTemp));
                        if (chanceOfEmber > .55f ){ //
                            float3 newPos = new float3(translation.Value);
                            newPos = new float3((float)newPos.x+(float)r.NextFloat(-.5f,.5f),
                                (float)newPos.y+(float)r.NextFloat(.5f,.6f), 
                                (float)newPos.z+(float)r.NextFloat(-.5f,.5f));

                            
                            EmberLifeCycleSystem.createEmber(
                                ecb,
                                emberArchetype,
                                newPos,
                                Quaternion.Euler(0, 0, 0),
                                sphereMesh,
                                fireMaterial,
                                new float3(r.NextFloat(0,.5f), r.NextFloat(0,6f), r.NextFloat(0,.5f)), //initial velocity
                                r.NextFloat(temperature.Value,temperature.Value*10) //temperature
                                );
                        }
                    }
                })
                .Run();
        }


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

    private static float sigmoid(float a, float f){
        return 1f / (1f + Mathf.Exp(-a*f));
    }

    private static unsafe Entity createEmber(
        EntityCommandBuffer ecb,
        EntityArchetype archetype,
        float3 position, 
        quaternion orientation,
        Mesh mesh,
        UnityEngine.Material material,
        float3 velocity,
        float temperature){


        Entity entity = ecb.CreateEntity(archetype);
        
        //Transform stuff
        ecb.SetComponent(entity, new Translation { Value = position });
        ecb.SetComponent(entity, new Rotation { Value = orientation });
        ecb.SetComponent(entity, new Scale { Value = .0625f });
        // ecb.SetComponent(entity, new Scale { Value = .5f });

        //custom stuff
        int igniteTemp = (int)TemperatureSystem.igniteTemp;
        ecb.SetComponent(entity, new Temperature{ Value = temperature, Rate = igniteTemp });   

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
