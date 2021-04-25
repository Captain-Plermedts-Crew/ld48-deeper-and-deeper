using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct WeaponComponent : IComponentData
{
    public float rateOfFire;

    public float shotTimer;
}
