using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

using System;
using Random = Unity.Mathematics.Random;

// [UpdateInGroup(typeof(InitializationSystemGroup))]
public class LevelSetupSystem : SystemBase
{
    private Random r = new Random(123);
    private EntityManager entityManager;

    protected override void OnStartRunning()
    {
            base.OnStartRunning();
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // createMouseEntity(entityManager);
    }
      
    protected override void OnUpdate()
    {
      
    }

    protected void createMouseEntity(EntityManager entityManager){

        EntityArchetype mouseArchetype = entityManager.CreateArchetype(
            typeof(MouseTag),
            typeof(Translation),
            typeof(Rotation),
            typeof(LocalToWorld));

        Entity mouseEntity = entityManager.CreateEntity(mouseArchetype);
        
        entityManager.AddComponentData(mouseEntity, new Translation { Value = float3.zero });
        entityManager.AddComponentData(mouseEntity, new MouseTag{});  
        entityManager.AddComponentData(mouseEntity, new Temperature{ Value = 0f });      

    }


}