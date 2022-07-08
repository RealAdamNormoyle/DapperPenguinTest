using Unity.Entities;
using Unity.Mathematics;

public struct GridCell : IComponentData {
    public int Index;
    public float2 GridPosition;
    public float3 WorldPosition;
    public bool IsBlocked;
}
