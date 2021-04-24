using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct Temperature : IComponentData
{
    public float tempLossRate;
    public float temperature;
}
