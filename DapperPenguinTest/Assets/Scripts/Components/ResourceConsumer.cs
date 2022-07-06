using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ResourceConsumer : IComponentData
{
    public bool IsVehicleDispatched { get; set; }
}
