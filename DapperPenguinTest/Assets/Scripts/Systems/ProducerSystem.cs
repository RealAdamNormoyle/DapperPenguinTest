using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ProducerSystem : ComponentSystem {

    protected EntityArchetype m_producerBuildingArchetype;
    int m_producerCount;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    protected override void OnUpdate() {
        float dtime = Time.DeltaTime;
        EntityManager entityManager = EntityManager;

        NativeArray<Entity> consumerEntities = GetEntityQuery(ComponentType.ReadOnly<ResourceConsumer>()).ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity e, ref ResourceProducer producer, ref ResourceStorage storage) => {

            //update production progress
            producer.ProductionProgress += dtime;
            if (producer.ProductionProgress >= producer.ProductionRate) {
                producer.ProductionProgress -= producer.ProductionRate;
                storage.Value++;
            }

            DynamicBuffer<IntBufferElement> dependentConsumerIds = GetBufferFromEntity<IntBufferElement>()[e];
            // We are not delivering to anyone so early out
            if (dependentConsumerIds.Length == 0)
                return;

            DynamicBuffer<int> dependentConsumerIdsInt = dependentConsumerIds.Reinterpret<int>();

            //equally serve consumers
            if (storage.Value >= 1) {
                producer.LastConsumerServed++;
                if (producer.LastConsumerServed == dependentConsumerIdsInt.Length)
                    producer.LastConsumerServed = 0;

                for (int i = 0; i < consumerEntities.Length; i++) {
                    ResourceConsumer consumerData = entityManager.GetComponentData<ResourceConsumer>(consumerEntities[i]);
                    if (consumerData.ConsumerId == dependentConsumerIdsInt[producer.LastConsumerServed]) {
                        // we expect a vehicle to collect a resource so take it out of storage and track it as expectedOut
                        if (!consumerData.IsVehicleDispatched && !consumerData.ProducerReadyForVehicle) {
                            storage.Value--;
                            storage.ExpectedOut++;
                            consumerData.ProducerReadyForVehicle = true;
                        }
                        entityManager.SetComponentData(consumerEntities[i], consumerData);
                    }

                }
            }
        });

        consumerEntities.Dispose();
    }


    void RegisterArchetypes() {
        m_producerBuildingArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(ResourceStorage),
            typeof(ResourceProducer),
            typeof(WorldEntity)
            );
    }

    public void SpawnProducer(float2 _position,Mesh _mesh,Material _material,int _productionRate) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.CreateEntity(m_producerBuildingArchetype);

        entityManager.SetComponentData(entity, new Translation{ Value = new float3 { x = (int)_position.x, y = (int)_position.y, z = -1} });
        entityManager.SetComponentData(entity, new ResourceProducer {ProducerId = m_producerCount++, ProductionRate = _productionRate, ProductionProgress = 0 });
        entityManager.SetComponentData(entity, new ResourceStorage());
        entityManager.SetComponentData(entity, new WorldEntity { BlocksGrid = true, GridPosition = new float2 { x = (int)_position.x, y = (int)_position.y } });
        entityManager.AddBuffer<IntBufferElement>(entity);
        entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = _mesh, material = _material });
    }

    internal void OnGUI() {
        Entities.ForEach((Entity e, ref ResourceProducer producer, ref ResourceStorage storage, ref Translation translation) => {
            var position = Camera.main.WorldToScreenPoint(translation.Value);
            GUI.Label(new Rect(new Vector2 { x = position.x, y = Camera.main.pixelHeight - position.y},new Vector2 { x = 100f,y = 40f}),$"P-{producer.ProducerId+1} (Storage = {storage.Value + storage.ExpectedOut})");
        });
    }
}
