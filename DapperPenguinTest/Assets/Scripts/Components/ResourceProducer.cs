using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct ResourceProducer : IComponentData {

    public int ProducerId;
    public float ProductionProgress;
    public int ProductionRate;
    public int LastConsumerServed;
    
}
