using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ProducerSystem : SystemBase {

    protected EntityArchetype m_producerBuildingArchetype;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    protected override void OnUpdate() {
        float dtime = Time.DeltaTime;
        Entities.ForEach((ref ResourceProducer producer, ref ResourceStorage storage) => {
            producer.ProductionProgress += dtime;
            if(producer.ProductionProgress >= 1f) {
                producer.ProductionProgress -= 1f;
                storage.Value += producer.ProductionRate;
            }
        }).Run();
    }


    void RegisterArchetypes() {
        m_producerBuildingArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(GridPosition),
            typeof(ResourceStorage),
            typeof(ResourceProducer),
            typeof(WorldEntity)
            );
    }

    public void SpawnProducer(float2 _position,Mesh _mesh,Material _material,int _productionRate) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.CreateEntity(m_producerBuildingArchetype);

        entityManager.SetComponentData(entity, new GridPosition() { Value = new float2 { x = (int)_position.x, y = (int)_position.y } });
        entityManager.SetComponentData(entity, new Translation() { Value = new float3 { x = (int)_position.x, y = (int)_position.y, z = -1} });
        entityManager.SetComponentData(entity, new ResourceProducer { ProductionRate = _productionRate });
        entityManager.SetComponentData(entity, new WorldEntity { BlocksGrid = true });
        entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = _mesh, material = _material });
    }
}
