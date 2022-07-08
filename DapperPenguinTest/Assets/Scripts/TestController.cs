using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Assertions;

public class TestController : MonoBehaviour
{

    /// <summary>
    /// //Test properties, you would usually setup scriptable objects for stuff like this but its a bit overkill for what we are doing here.
    /// </summary>
    /// 

    /// <summary>
    /// // Base mesh used by all entities
    /// </summary>
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
    [SerializeField] private int m_producerProductionTime = 1;
    [SerializeField] private Material m_producerMaterial = null;

    [Space]
    [Header("Consumer Properties", order = 3)]
    [Range(1, 64)]
    [SerializeField] private int m_consumerCount = 5;
    [SerializeField] private Material m_consumerMaterial = null;

    [Space]
    [Header("Vehicle Properties", order = 4)]
    [SerializeField] private Material m_vehicleMaterial = null;

    [Space]
    [Header("Road Properties", order = 5)]
    [SerializeField] private Material m_roadMaterial = null;

    private GridSystem m_gridSystem = null;
    private ProducerSystem m_producerSystem = null;
    private ConsumerSystem m_consumerSystem = null;
    private RoadSystem m_roadSystem = null;
    private VehicleSystem m_vehicleSystem = null;

    private void OnValidate() {
        Assert.IsNotNull(m_squareMesh);
        Assert.IsNotNull(m_gridCellMaterial);
        Assert.IsNotNull(m_producerMaterial);
        Assert.IsNotNull(m_consumerMaterial);
        Assert.IsNotNull(m_vehicleMaterial);
        Assert.IsNotNull(m_roadMaterial);
    }


    //Main Intialization
    void Start() {
        m_gridSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(GridSystem)) as GridSystem;
        m_gridSystem.GenerateGrid(m_gridWidth, m_gridHeight, m_gridCellMaterial, m_squareMesh);
        m_vehicleSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(VehicleSystem)) as VehicleSystem;

        m_producerSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(ProducerSystem)) as ProducerSystem;
        SpawnRandomProducers();

        m_consumerSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(ConsumerSystem)) as ConsumerSystem;
        SpawnRandomConsumers();

        m_roadSystem = World.DefaultGameObjectInjectionWorld.CreateSystem(typeof(RoadSystem)) as RoadSystem;
        m_roadSystem.GenerateRoads(new float2(m_gridWidth, m_gridHeight), m_squareMesh, m_roadMaterial);

        m_vehicleSystem.GeneratePaths(m_gridWidth,m_gridHeight);

        Camera.main.transform.position = new Vector3(m_gridWidth / 2, m_gridHeight / 2, -10);
    }

    public void OnGUI() {
        m_producerSystem.OnGUI();
        m_consumerSystem.OnGUI();
    }

    /// <summary>
    /// // Spawn all Produers randomly on the grid
    /// </summary>
    public void SpawnRandomProducers() {
        float2 position;
        for (int i = 0; i < m_producerCount; i++) {
            position.x = UnityEngine.Random.Range(0, m_gridWidth);
            position.y = UnityEngine.Random.Range(0, m_gridHeight);
            m_producerSystem.SpawnProducer(position, m_squareMesh, m_producerMaterial, m_producerProductionTime);
        }
    }

    /// <summary>
    /// // Spawn all Consumers randomly on the grid
    /// </summary>
    public void SpawnRandomConsumers() {
        float2 position;
        for (int i = 0; i < m_consumerCount; i++) {
            position.x = UnityEngine.Random.Range(0, m_gridWidth);
            position.y = UnityEngine.Random.Range(0, m_gridHeight);
            m_consumerSystem.SpawnRandomConsumerWithVehicle(position, m_squareMesh, m_consumerMaterial,m_vehicleMaterial);
        }
    }

}
