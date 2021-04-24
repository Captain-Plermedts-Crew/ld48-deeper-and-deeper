using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;

using Collider = Unity.Physics.Collider;
using MeshCollider = Unity.Physics.MeshCollider;
using System;
using Random = Unity.Mathematics.Random;

public class EntityUtils {

    public static ComponentType[] getRenderedPhysicsComponents(bool isDynamic){
        return mergeComponentTypes(getRenderComponents(), getPhysicsComponents(isDynamic));
    }
   
    public static ComponentType[] getRenderComponents(){
        return new ComponentType[]{
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(Translation),
            typeof(Rotation),
            typeof(LocalToWorld)};
    }

     public static ComponentType[] getPhysicsComponents(bool isDynamic){
        ComponentType[] componentTypes = new ComponentType[isDynamic ? 5 : 1];
        componentTypes[0] = typeof(PhysicsCollider);
        if (isDynamic){
            componentTypes[1] = typeof(PhysicsVelocity);
            componentTypes[2] = typeof(PhysicsMass);
            componentTypes[3] = typeof(PhysicsDamping);
            componentTypes[4] = typeof(PhysicsGravityFactor);
        }
        return componentTypes;
    }

     public static ComponentType[] mergeComponentTypes(ComponentType[] array1, ComponentType[] array2){        
        ComponentType[] newArray = new ComponentType[array1.Length + array2.Length];
        Array.Copy(array1, newArray, array1.Length);
        Array.Copy(array2, 0, newArray, array1.Length, array2.Length);
        return newArray;
    }

    // public static unsafe Entity CreateBody(
    //     EntityManager entityManager,
    //     RenderMesh displayMesh, 
    //     float3 position, 
    //     quaternion orientation,
    //     BlobAssetReference<Collider> collider,
    //     float3 linearVelocity, 
    //     float3 angularVelocity,
    //     float mass,
    //     bool isDynamic){
            
    //     ComponentType[] componentTypes = new ComponentType[isDynamic ? 9 : 6];

    //     componentTypes[0] = typeof(RenderMesh);
    //     componentTypes[1] = typeof(RenderBounds);
    //     componentTypes[2] = typeof(Translation);
    //     componentTypes[3] = typeof(Rotation);
    //     componentTypes[4] = typeof(LocalToWorld);
    //     componentTypes[5] = typeof(PhysicsCollider);
    //     if (isDynamic) {
    //         componentTypes[6] = typeof(PhysicsVelocity);
    //         componentTypes[7] = typeof(PhysicsMass);
    //         componentTypes[8] = typeof(PhysicsDamping);
    //     }
    //     Entity entity = entityManager.CreateEntity(componentTypes);

    //     entityManager.SetSharedComponentData(entity, displayMesh);
    //     entityManager.SetComponentData(entity, new RenderBounds { Value = displayMesh.mesh.bounds.ToAABB() });

    //     entityManager.SetComponentData(entity, new Translation { Value = position });
    //     entityManager.SetComponentData(entity, new Rotation { Value = orientation });

    //     entityManager.SetComponentData(entity, new PhysicsCollider { Value = collider });

    //     if (isDynamic){
    //         Collider* colliderPtr = (Collider*)collider.GetUnsafePtr();
    //         entityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));
    //         // Calculate the angular velocity in local space from rotation and world angular velocity
    //         float3 angularVelocityLocal = math.mul(math.inverse(colliderPtr->MassProperties.MassDistribution.Transform.rot), angularVelocity);
            
    //         entityManager.SetComponentData(entity, new PhysicsVelocity(){Linear = linearVelocity, Angular = angularVelocityLocal});

    //         entityManager.SetComponentData(entity, new PhysicsDamping(){Linear = 0.01f, Angular = 0.05f});
    //     }

    //     return entity;
    
    // }
}