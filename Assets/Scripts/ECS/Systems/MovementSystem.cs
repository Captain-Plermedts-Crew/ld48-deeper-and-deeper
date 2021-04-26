using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

[UpdateAfter(typeof(GatherInputSystem))]
public class MovePlayerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .WithoutBurst()
            .WithAll<Movement, UserInputData>()
            .ForEach((ref Translation translation, in Movement movement, in UserInputData input) =>
            {
                translation.Value += new float3(input.Move.x, 0.0f, input.Move.y) * movement.speed * deltaTime;
            })
            .ScheduleParallel();
    }
}
