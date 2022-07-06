using System;
using Unity.Entities;
using Unity.Mathematics;

public struct GridPosition : IComponentData, IEquatable<GridPosition>
{
    public float2 Value;
    public bool IsBlocked;

    //We only want to compare the positions here
    public bool Equals(GridPosition other) {
        return Value.Equals(other.Value);
    }
}
