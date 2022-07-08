using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public class ConsumerSystem : ComponentSystem {
    protected EntityArchetype m_consumerBuildingArchetype;
    int m_consumerCount;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    protected override void OnUpdate() {
        EntityManager entityManager = EntityManager;
        NativeArray<Entity> vehicles = entityManager.CreateEntityQuery((typeof(Vehicle))).ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity e,ref ResourceConsumer consumer, ref ResourceStorage storage,ref WorldEntity worldEntity) => {
            if(!consumer.IsVehicleDispatched && consumer.ProducerReadyForVehicle) {
                foreach (var item in vehicles) {                      
                    var data = GetComponentDataFromEntity<Vehicle>()[item];
                    if(data.VehicleId == consumer.ConsumerId) {
                        data.IsActive = true;
                        consumer.IsVehicleDispatched = true;
                        consumer.ProducerReadyForVehicle = false;
                        entityManager.SetComponentData(item, data);
                    }
                }
            }
        });
        vehicles.Dispose();
    }


    void RegisterArchetypes() {
        m_consumerBuildingArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(ResourceStorage),
            typeof(ResourceConsumer),
            typeof(WorldEntity)
            );
    }

    public void SpawnRandomConsumerWithVehicle(float2 _bounds, Mesh _mesh, Material _material,Material _vehicleMaterial) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = entityManager.CreateEntity(m_consumerBuildingArchetype);

        float2 gridPosition = new float2 { 
            x = (int)UnityEngine.Random.Range(0,_bounds.x), 
            y = (int)UnityEngine.Random.Range(0,_bounds.y) 
        };


        //Find the closest producer
        NativeArray<Entity> producers = GetEntityQuery(ComponentType.ReadOnly<ResourceProducer>()).ToEntityArray(Allocator.TempJob);
        int bestId = 0;
        float bestDist = 100000;
        for (int i = 0; i < producers.Length; i++) {
            var data = entityManager.GetComponentData<WorldEntity>(producers[i]);
            var dist = Vector2.Distance(data.GridPosition, gridPosition);
            if (dist < bestDist) {
                bestDist = dist;
                bestId = i;
            }
        }

        var producer = entityManager.GetComponentData<ResourceProducer>(producers[bestId]);
        var producerWorldEntity = entityManager.GetComponentData<WorldEntity>(producers[bestId]);

        //add consumerId to  producer dependentIds buffer
        DynamicBuffer<IntBufferElement> consumerIds = GetBufferFromEntity<IntBufferElement>()[producers[bestId]];
        consumerIds.Add(new IntBufferElement { Value = m_consumerCount });
        
        var consumer = new ResourceConsumer { ConsumerId = m_consumerCount, IsVehicleDispatched = false, TargetProducerId = producer.ProducerId };

        entityManager.SetComponentData(producers[bestId], producer);
        entityManager.SetComponentData(entity, new Translation{ Value = new float3 { x = (int)gridPosition.x, y = (int)gridPosition.y, z = -1 } });
        entityManager.SetComponentData(entity, consumer);
        entityManager.SetComponentData(entity, new WorldEntity { BlocksGrid = true, GridPosition = new float2 { x = (int)gridPosition.x, y = (int)gridPosition.y } });
        entityManager.SetComponentData(entity, new ResourceStorage());
        entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = _mesh, material = _material });

        entityManager.World.GetExistingSystem<VehicleSystem>().SpawnVehicle(m_consumerCount, _mesh, _vehicleMaterial, new float3[] { new float3 { x = (int)gridPosition.x, y = (int)gridPosition.y }, new float3 { x = producerWorldEntity.GridPosition.x, y = producerWorldEntity.GridPosition.y, z = 0 } }, producer.ProducerId) ;

        m_consumerCount++;

        producers.Dispose();
    }

    //debug text for visualizations
    internal void OnGUI() {
        Entities.ForEach((Entity e, ref ResourceConsumer consumer, ref ResourceStorage storage, ref Translation translation) => {
            var position = Camera.main.WorldToScreenPoint(translation.Value);
            GUI.Label(new Rect(new Vector2 { x = position.x, y = Camera.main.pixelHeight - position.y }, new Vector2 { x = 100f, y = 40f }), $"C-{consumer.ConsumerId+1} (Storage = {storage.Value})");
        });
    }
}
