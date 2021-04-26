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

public class EnemySpawner : MonoBehaviour
{
    private Mesh cubeMesh;
    private UnityEngine.Material iceMaterial;

    private Random r = new Random(123);

    private float halfLevelSize = 50;     
    private EntityManager entityManager;
    private EntityArchetype frozenEnemyArchetype;

    // Awake is called before the first frame update
    // Need to use an Awake instead of a Start because GameManager calls spawn in their start... 
    // might be a better way to trigger spawn events?
    void Awake()
    {
        var cubeGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeMesh = cubeGO.GetComponent<MeshFilter>().mesh;
        Destroy(cubeGO);
        iceMaterial = Resources.Load("IceMaterial", typeof(UnityEngine.Material)) as UnityEngine.Material;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        frozenEnemyArchetype = EnemySpawner.getFrozenEntityArchetype(entityManager);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn(int numEnemies){

        for (int i=0; i<numEnemies; i++){
            float distance = r.NextFloat(10f, 90f);
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle.normalized * distance;
            float3 position = new float3(randomPoint.x, 1.1f, randomPoint.y);            

            createSingleEnemy(
                entityManager,
                frozenEnemyArchetype,
                position, //position
                Quaternion.Euler(0,r.NextFloat(0,360),0), //orientation
                cubeMesh,
                iceMaterial);
        }
    }

    
    private unsafe static Entity createSingleEnemy(
        EntityManager entityManager,
        EntityArchetype archetype,
        float3 position, 
        quaternion orientation,
        Mesh mesh,
        UnityEngine.Material material){

        Entity entity = entityManager.CreateEntity(archetype);
        
        //Transform stuff
        entityManager.AddComponentData(entity, new Translation { Value = position });
        entityManager.SetComponentData(entity, new Rotation { Value = orientation });

        //custom stuff
        entityManager.AddComponentData(entity, new Temperature{ Value = 0f });   

        //RenderMesh & Bounds   
        entityManager.AddSharedComponentData(entity, new RenderMesh 
		{
			mesh = mesh,
			material = material
		});
        entityManager.SetComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

        Unity.Physics.Material physicsMaterial = new Unity.Physics.Material{
            CollisionResponse = Unity.Physics.CollisionResponsePolicy.CollideRaiseCollisionEvents
        };
        

        BlobAssetReference<Collider> collider = Unity.Physics.SphereCollider
            .Create(new SphereGeometry {
                Center = float3.zero,
                Radius = 1,
            },
            CollisionFilter.Default,
            physicsMaterial
        );

        entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });

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
