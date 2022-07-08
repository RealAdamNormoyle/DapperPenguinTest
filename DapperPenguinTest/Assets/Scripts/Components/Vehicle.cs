using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Vehicle : IComponentData
{
    public int VehicleId;
    public int ActivePathNode;
    public bool IsActive;
    public float Speed;
    public bool IsMovingToProducer;
    public int ProducerId;
}
