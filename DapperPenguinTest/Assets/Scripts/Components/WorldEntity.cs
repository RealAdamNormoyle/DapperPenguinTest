using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct WorldEntity : IComponentData {
    public bool BlocksGrid { get; set; }
}
