using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Extensions;

public class RestrictPlayerRotationSystem : SystemBase
{
     protected override void OnUpdate(){
        Entities
            .WithAll<PlayerTag>()
            .ForEach((ref Rotation rot) =>
            {
                //TODO lock rot x & z here
            })
            .Run();
    }
}
