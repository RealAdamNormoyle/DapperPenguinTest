using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct WorldEntity : IComponentData {
    public float2 GridPosition;
    public bool BlocksGrid;
}
