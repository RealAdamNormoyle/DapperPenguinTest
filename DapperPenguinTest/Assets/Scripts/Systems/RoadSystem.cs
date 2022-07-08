using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


[BurstCompile]
public class RoadSystem : JobComponentSystem {

    EntityArchetype m_roadEntityArchetype;

    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();
    }

    private void RegisterArchetypes() {
        m_roadEntityArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(WorldEntity),
            typeof(Road)
            );
    }

    /// <summary>
    /// Generate connecting roads for all buildings, this is by far not the best way to do this but it was the quickest at the time. You might want to try a flood fill algo to get a nicer visual
    /// </summary>
    public void GenerateRoads(float2 _gridSize,Mesh _roadMesh,Material _roadMaterial) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        NativeArray<WorldEntity> buildings = GetEntityQuery(ComponentType.ReadOnly<WorldEntity>()).ToComponentDataArray<WorldEntity>(Allocator.Temp);

        // Find a line splitting all buildings in two groups
        float sum = 0;
        for (int i = 0; i < buildings.Length; i++) {
            sum += buildings[i].GridPosition.x;
        }
        int average = (int)(sum / buildings.Length);


        // Generate a 'main' road on the center line
        NativeArray<Entity> mainRoads = entityManager.CreateEntity(m_roadEntityArchetype,(int)_gridSize.y,Allocator.Temp);
        for (int i = 0; i < mainRoads.Length; i++) {
            entityManager.SetComponentData(mainRoads[i], new Translation() { Value = new float3 { x = average, y = i, z = -0.1f } });
            entityManager.SetComponentData(mainRoads[i], new WorldEntity() { GridPosition = new float2 { x = average, y = i } });
            entityManager.SetComponentData(mainRoads[i], new Road() { GridPosition = new float2 { x = average, y = i } });
            entityManager.SetSharedComponentData(mainRoads[i], new RenderMesh { mesh = _roadMesh, material = _roadMaterial });
        }
        
        //Generate 'side' roads connecting each building to the main road 
        for (int i = 0; i < buildings.Length; i++) {
            int min = (int)math.min(buildings[i].GridPosition.x, average);
            int max = (int)math.max(buildings[i].GridPosition.x, average);
            int dist = max - min;
            NativeArray<Entity> sideRoads = entityManager.CreateEntity(m_roadEntityArchetype, dist, Allocator.Temp);
            for (int j = 1; j < dist; j++) {
                entityManager.SetComponentData(sideRoads[j], new Translation() { Value = new float3 { x = min + j, y = buildings[i].GridPosition.y, z = -0.1f } });
                entityManager.SetComponentData(sideRoads[j], new WorldEntity() { GridPosition = new float2 { x = min + j, y = buildings[i].GridPosition.y } });
                entityManager.SetComponentData(sideRoads[j], new Road() { GridPosition = new float2 { x = min + j, y = buildings[i].GridPosition.y } });
                entityManager.SetSharedComponentData(sideRoads[j], new RenderMesh { mesh = _roadMesh, material = _roadMaterial });
            }

            sideRoads.Dispose();

        }

        mainRoads.Dispose();
        buildings.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        return inputDeps;
    }
}
