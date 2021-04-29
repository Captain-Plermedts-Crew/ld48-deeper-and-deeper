using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Extensions;

using Random = Unity.Mathematics.Random;

public class RandomBreezeSystem : SystemBase {

    private static Random r = new Random(123123);

    public static float3 currentWindForce;
    public static float currentWindDirection = 0f; //radians
    public static float forceScalar = .125f;

    public static float windRateOfChangePerFrame = 1f;
    public static float perEmberForceRange = .125f;

    protected override void OnCreate() {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        /*Notes: Algorithm:
            1) Add the current wind direction [radians] to a randomly generated float between -1 and 1.
            2) Convert direction into a vector using trig. the y-value is the sum of the x and z-values to simulate up/down drafts
            3) apply the force to each ember
                3a) Also add a randomly generated force to each ember to add variety

            Knobs: the rate of change of wind direction, the value of force.y (up/down), force scalar, and the randomly generated per-ember force
        */
        float deltaTime = Time.DeltaTime;
        currentWindDirection = (currentWindDirection + r.NextFloat(-windRateOfChangePerFrame, windRateOfChangePerFrame)) % (2*Mathf.PI);// (c +/- 1) %2PI
        float sin = math.sin(currentWindDirection);
        float cos = math.cos(currentWindDirection);
        currentWindForce = new float3(cos, cos+sin, sin) * r.NextFloat(forceScalar); //x and z are normal rotation matrix, but y is just for funsies


        Entities
            .WithoutBurst()
            .WithAll<EmberTag>()
            .ForEach((ref PhysicsVelocity physicsVelocity, ref PhysicsMass physicsMass) => 
            {
                PhysicsComponentExtensions.ApplyLinearImpulse(
                    ref physicsVelocity,
                    physicsMass, 
                    currentWindForce + r.NextFloat3(-perEmberForceRange, perEmberForceRange)); 
                
                PhysicsComponentExtensions.ApplyAngularImpulse(
                    ref physicsVelocity, 
                    physicsMass, 
                    r.NextFloat3(-.001f, .001f)); //apply angular impulse along the axis with the given amount

            })
            .Run();

    }
}
