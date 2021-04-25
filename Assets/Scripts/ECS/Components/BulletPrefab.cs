using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

 [GenerateAuthoringComponent]
public struct BulletPrefab : IComponentData
{
    public Entity Value;
}