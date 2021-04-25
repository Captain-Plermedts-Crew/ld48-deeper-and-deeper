using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct UserInputData : IComponentData
{
    public float2 Move;
}
