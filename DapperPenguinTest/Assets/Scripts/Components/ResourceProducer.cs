using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ResourceProducer : IComponentData {

    public float ProductionProgress;
    public int ProductionRate;
}
