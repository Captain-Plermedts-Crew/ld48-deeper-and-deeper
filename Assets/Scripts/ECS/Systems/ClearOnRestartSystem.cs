using Unity.Entities;

namespace DroneSwarm
{
    /// <summary>
    /// Assigns Lifetime components to any enemies on game over,
    /// removing any leftover drones when the scene restarts.
    /// </summary>
    public class ClearOnRestartSystem : ComponentSystem
    {
        // approximate the screen fader transition time
        float endLifetime = 2f;

        protected override void OnUpdate()
        {
            // check if the game is over
            if (GameManager.IsGameOver())
            {
                // default EntityManager
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                // find all resetables via temperature
                // probably better way to get resetables...
                Entities.WithAny<Temperature>()
                    .WithNone<Lifetime, PlayerTag, WeaponComponent>()
                    .ForEach((Entity enemy) =>
                    {
                        // add the Lifetime component to time out Entity automatically
                        entityManager.AddComponentData(enemy, new Lifetime { Value = endLifetime });
                    });
            }
        }
    }
}