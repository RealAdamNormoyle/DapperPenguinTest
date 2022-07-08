using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Jobs;
using Unity.Burst;

[BurstCompile]
public partial class GridSystem : JobComponentSystem {

    protected EntityArchetype m_gridCellArchetype;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    void RegisterArchetypes() {
        m_gridCellArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(GridCell)
            );
    }

    public void GenerateGrid(int _width, int _height, Material _cellMaterial, Mesh _cellMesh) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entities = entityManager.CreateEntity(m_gridCellArchetype, _width * _height, Allocator.Temp);
        for (int i = 0; i < entities.Length; i++) {
            int gridY = i / _width;
            int gridX = i % _height;
            entityManager.SetComponentData(entities[i], new GridCell() { Index = i, GridPosition = new float2 { x = gridX, y = gridY }, WorldPosition = new float3 { x = gridX, y = gridY, z = 0 } });
            entityManager.SetComponentData(entities[i], new Translation() { Value = new float3 { x = gridX, y = gridY, z = 0 } });
            entityManager.SetSharedComponentData(entities[i], new RenderMesh { mesh = _cellMesh, material = _cellMaterial });
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        return inputDeps;
    }
}
