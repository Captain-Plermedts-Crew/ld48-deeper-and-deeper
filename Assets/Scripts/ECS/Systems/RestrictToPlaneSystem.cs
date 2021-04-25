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
            .ForEach((ref Translation pos, 
                ref PhysicsVelocity physicsVelocity) =>
            {
                pos.Value.y = 2;
                physicsVelocity.Linear.y=0;
            })
            .Run();
    }
}
