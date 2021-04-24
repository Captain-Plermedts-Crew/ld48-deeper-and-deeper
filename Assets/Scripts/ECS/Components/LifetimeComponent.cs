using Unity.Entities;
/// <summary>
/// Defines a duration for removing an Entity.
/// </summary>
[GenerateAuthoringComponent]
public struct Lifetime : IComponentData
{
    public float Value;
}
