using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Extensions;

public class RestrictToPlaneSystem : SystemBase
{
     protected override void OnUpdate(){
        Entities
            .WithAll<Translation, Rotation>()
            .WithNone<EmberTag>()
            .ForEach((ref Translation pos, 
                ref PhysicsVelocity physicsVelocity) =>
            {
                pos.Value.y = 1.75f;
                physicsVelocity.Linear.y=0;
            })
            .Run();
    }
}
