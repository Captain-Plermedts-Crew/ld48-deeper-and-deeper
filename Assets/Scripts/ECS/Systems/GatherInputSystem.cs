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
        var isFiring = Input.GetButton("Fire1");

        Entities
            .WithName("GatherInput")
            .ForEach((ref UserInputData inputData) =>
            {
                inputData.Move = new float2(horizontal, vertical);
                inputData.IsFiring = isFiring;
            })
            .ScheduleParallel();
    }
}
