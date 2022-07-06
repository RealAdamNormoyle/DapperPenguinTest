using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ConsumerSystem : SystemBase {
    protected EntityArchetype m_consumerBuildingArchetype;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    protected override void OnUpdate() {
        Entities.ForEach((ref ResourceProducer producer, ref ResourceStorage storage) => {

        }).Run();
    }


    void RegisterArchetypes() {
        m_consumerBuildingArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(GridPosition),
            typeof(ResourceStorage),
            typeof(ResourceConsumer),
            typeof(WorldEntity)
            );
    }

    public void SpawnConsumer(float2 _position, Mesh _mesh, Material _material) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.CreateEntity(m_consumerBuildingArchetype);

        GridPosition gridPosition = new GridPosition() { Value = new float2 { x = (int)_position.x, y = (int)_position.y } };

        entityManager.SetComponentData(entity, gridPosition);
        entityManager.SetComponentData(entity, new Translation() { Value = new float3 { x = (int)_position.x, y = (int)_position.y, z = -1 } });
        entityManager.SetComponentData(entity, new ResourceConsumer { IsVehicleDispatched = false });
        entityManager.SetComponentData(entity, new WorldEntity { BlocksGrid = true });
        entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = _mesh, material = _material });
    }
}
