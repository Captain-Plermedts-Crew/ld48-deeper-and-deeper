using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct Temperature : IComponentData
{
    public float Rate;
    public float Value;
}
