using Unity.Entities;

public class TimeoutSystem : ComponentSystem
{
    // runs every frame
    protected override void OnUpdate()
    {
        // loop through all entites with a Lifetime component
        Entities.WithAll<Lifetime>().ForEach((Entity entity, ref Lifetime lifetime) =>
        {
            // decrement by time elapsed for one frame
            lifetime.Value -= Time.DeltaTime;

            // if we have timed out, remove the Entity safely
            if (lifetime.Value <= 0)
            {
                PostUpdateCommands.DestroyEntity(entity);
            }

        });
    }
}
