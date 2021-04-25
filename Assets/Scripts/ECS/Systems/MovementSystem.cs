using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

public partial class GatherInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        Entities
            .WithName("GatherInput")
            .ForEach((ref UserInputData inputData) =>
            {
                inputData.Move = new float2(horizontal, vertical);
            }).ScheduleParallel();
    }
}

[UpdateAfter(typeof(GatherInputSystem))]
public class MovePlayerSystem : SystemBase
{
    // 2 event-style callback that runs every frame
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        // 3 loop through all Entities with MoveForward component; pass in Translation/Rotation/MoveForward components as input parameters
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
