using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

/// <summary>
/// System for moving enemies or bullets forward
/// </summary>
/// 
// 1 Systems inherit from ComponentSystem (single thread) 
public class MovementSystem : ComponentSystem
{
    // 2 event-style callback that runs every frame
    protected override void OnUpdate()
    {
        // 3 loop through all Entities with MoveForward component; pass in Translation/Rotation/MoveForward components as input parameters
        Entities.WithAll<MoveForward>().ForEach((ref Translation trans, ref Rotation rot, ref MoveForward moveForward) =>
        {
            // 4 calculate how much to move each frame in the local positive z and increment the position
            trans.Value += moveForward.speed * Time.DeltaTime * math.forward(rot.Value);
        });
    }
}

// Systems can also be multithreaded by inheriting from JobComponentSystem; 
// for more info see:
// https://docs.unity3d.com/Packages/com.unity.entities@0.1/manual/job_component_system.html
