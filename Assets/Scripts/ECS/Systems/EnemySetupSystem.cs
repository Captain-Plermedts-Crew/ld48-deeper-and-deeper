using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

using Unity.Physics;
using Collider = Unity.Physics.Collider;
using MeshCollider = Unity.Physics.MeshCollider;

using System;
using Random = Unity.Mathematics.Random;

// [UpdateInGroup(typeof(InitializationSystemGroup))]
public class EnemySetupSystem : SystemBase
{

    private Mesh cubeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh;//((GameObject)Resources.Load ("Models/SpaceFighterAlt")).GetComponent<MeshFilter>().sharedMesh;
    private UnityEngine.Material iceMaterial = Resources.Load("IceMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material;

    private Random r = new Random(123);

    private int numEnemies = 15;
    private float halfLevelSize = 50;
    private float enemySpawnRate = 1;
     
    private EntityManager entityManager;

    protected override void OnStartRunning()
    {
            base.OnStartRunning();
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            createLotsOfEnemies();
            
    }
      
    protected override void OnUpdate()
    {
      
    }

    private void createLotsOfEnemies(){

        for (int i=0; i<numEnemies; i++){
            float3 position = r.NextFloat3(-halfLevelSize,halfLevelSize);
            position.y = 1;

            createSingleEnemy(
                entityManager,
                position, //position
                Quaternion.Euler(0,r.NextFloat(0,360),0), //orientation
                cubeMesh,
                iceMaterial);
        }
        
    }

    private static Entity createSingleEnemy(
        EntityManager entityManager,
        float3 position, 
        quaternion orientation,
        Mesh mesh,
        UnityEngine.Material material){

        EntityArchetype frozenEnemyArchetype = getFrozenEntityArchetype(entityManager);

        Entity entity = entityManager.CreateEntity(frozenEnemyArchetype);
        
        //Transform stuff
        entityManager.AddComponentData(entity, new Translation { Value = position });
        entityManager.SetComponentData(entity, new Rotation { Value = orientation });

        //custom stuff
        entityManager.AddComponentData(entity, new Temperature{tempLossRate=1f, temperature=20f});   

        //RenderMesh & Bounds   
        entityManager.AddSharedComponentData(entity, new RenderMesh 
		{
			mesh = mesh,
			material = material
		});
        entityManager.SetComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

        BlobAssetReference<Collider> collider = Unity.Physics.SphereCollider
            .Create(new SphereGeometry {
                Center = float3.zero,
                Radius = 1,
            },
            CollisionFilter.Default
        );

        entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });

        // float mass = 5f;
        // Collider* colliderPtr = (Collider*)collider.GetUnsafePtr();
        // entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));
        // Calculate the angular velocity in local space from rotation and world angular velocity
        // float3 angularVelocityLocal = math.mul(math.inverse(colliderPtr->MassProperties.MassDistribution.Transform.rot), angularVelocity);
        // entityManager.SetComponentData(entity, new PhysicsVelocity()
        // {
        //     Linear = 0,
        //     Angular = 0
        // });
        // entityManager.SetComponentData(entity, new PhysicsDamping()
        // {
        //     Linear = 0.5f,
        //     Angular = 0.5f
        // });

        return entity;

    }


    public static EntityArchetype getFrozenEntityArchetype(EntityManager entityManager){
        ComponentType[] renderedPhysicsComponents = EntityUtils.getRenderedPhysicsComponents(false);
        ComponentType[] specificComponents = new ComponentType[]{
            typeof(EnemyTag),
            typeof(FrozenTag),
            typeof(Temperature)};
        return entityManager.CreateArchetype(EntityUtils.mergeComponentTypes(specificComponents, renderedPhysicsComponents));
    }   
}