using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Jobs;

public partial class GridSystem : SystemBase
{
    protected EntityArchetype m_gridCellArchetype;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    protected override void OnUpdate() {
        //TODO track and update grid for lookup later
        //EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //Entities.ForEach((ref GridPosition position, in WorldEntity worldEntity) => {
        //    bool blocks = worldEntity.BlocksGrid;
        //    position.IsBlocked = blocks;

        //}).Run();
    }

    void RegisterArchetypes() {
        m_gridCellArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(GridPosition)
            );
    }

    public void GenerateGrid(int _width, int _height, Material _cellMaterial, Mesh _cellMesh) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entities = entityManager.CreateEntity(m_gridCellArchetype, _width * _height, Allocator.Temp);
        for (int i = 0; i < entities.Length; i++) {
            int gridY = i / _width;
            int gridX = i % _width;

            entityManager.SetComponentData(entities[i], new GridPosition() { Value = new float2 { x = gridX, y = gridY } });
            entityManager.SetComponentData(entities[i], new Translation() { Value = new float3 { x = gridX, y = gridY, z = 0 } });
            entityManager.SetSharedComponentData(entities[i], new RenderMesh { mesh = _cellMesh, material = _cellMaterial });
        }
        entities.Dispose();
    }


}
