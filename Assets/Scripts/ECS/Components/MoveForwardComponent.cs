using Unity.Entities;
/// <summary>
/// Component data containing forward speed.
/// </summary>
[GenerateAuthoringComponent]
public struct MoveForward : IComponentData
{
    public float speed;
}
