using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ResourceConsumer : IComponentData
{
    public int ConsumerId;
    public bool IsVehicleDispatched;
    public bool ProducerReadyForVehicle;
    public int TargetProducerId;
    public int VehicleId;

}
