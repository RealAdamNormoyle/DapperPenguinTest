using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Assertions;

public class TestController : MonoBehaviour
{
    [Header("Shared Properties", order = 0)]
    [SerializeField] private Mesh m_squareMesh = null;

    [Space]
    [Header("Grid Properties", order = 1)]
    [Range(1, 1024)]
    [SerializeField] private int m_gridWidth = 256;
    [Range(1, 1024)]
    [SerializeField] private int m_gridHeight = 256;
    [SerializeField] private Material m_gridCellMaterial = null;

    [Space]
    [Header("Producer Properties", order = 2)]
    [Range(1, 64)]
    [SerializeField] private int m_producerCount = 3;
    [Range(0, 64)]
    [SerializeField] private int m_producerProductionPerSecond = 1;
    [SerializeField] private Material m_producerMaterial = null;

    [Space]
    [Header("Consumer Properties", order = 3)]
    [Range(1, 64)]
    [SerializeField] private int m_consumerCount = 5;
    [SerializeField] private Material m_consumerMaterial = null;

    [Space]
    [Header("Vehicle Properties", order = 4)]
    [Range(1, 5)]
    [SerializeField] private int m_vehiclesPerConsumer = 1;
    [SerializeField] private Material m_vehicleMaterial = null;
    [Range(0.1f, 1f)]
    [SerializeField] private float m_vehicleScale = 0.5f;

    [Space]
    [Header("Road Properties", order = 5)]
    [SerializeField] private Material m_roadMaterial = null;

    private GridSystem m_gridSystem = null;
    private ProducerSystem m_producerSystem = null;
    private ConsumerSystem m_consumerSystem = null;

    private void OnValidate() {
        Assert.IsNotNull(m_squareMesh);
        Assert.IsNotNull(m_gridCellMaterial);
        Assert.IsNotNull(m_producerMaterial);
        Assert.IsNotNull(m_consumerMaterial);
        Assert.IsNotNull(m_vehicleMaterial);
        Assert.IsNotNull(m_roadMaterial);
    }

    // Start is called before the first frame update
    void Start() {

        m_gridSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(GridSystem)) as GridSystem;
        m_gridSystem.GenerateGrid(256, 256, m_gridCellMaterial, m_squareMesh);

        m_producerSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(ProducerSystem)) as ProducerSystem;
        SpawnRandomProducers();

        m_consumerSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(ConsumerSystem)) as ConsumerSystem;
        SpawnRandomConsumers();
    }



    public void SpawnRandomProducers() {
        float2 position;
        for (int i = 0; i < m_producerCount; i++) {
            position.x = UnityEngine.Random.Range(0, m_gridWidth);
            position.y = UnityEngine.Random.Range(0, m_gridHeight);
            m_producerSystem.SpawnProducer(position, m_squareMesh, m_producerMaterial, m_producerProductionPerSecond);
        }
    }

    public void SpawnRandomConsumers() {
        float2 position;
        for (int i = 0; i < m_consumerCount; i++) {
            position.x = UnityEngine.Random.Range(0, m_gridWidth);
            position.y = UnityEngine.Random.Range(0, m_gridHeight);
            m_consumerSystem.SpawnConsumer(position, m_squareMesh, m_consumerMaterial);
        }
    }

}
