using UnityEngine;
using Unity.Entities;
/// <summary>
/// Component data containing forward speed.
/// </summary>
[GenerateAuthoringComponent]
public struct Movement : IComponentData
{
    public float speed;
}
