using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ResourceStorage : IComponentData {
    public int Value;
    public int ExpectedOut;
}
