using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class VehicleSystem : JobComponentSystem {
    protected EntityArchetype m_vehicleArchetype;
    
    protected override void OnCreate() {
        base.OnCreate();
        RegisterArchetypes();

    }

    void RegisterArchetypes() {
        m_vehicleArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(LocalToWorld),
            typeof(WorldEntity),
            typeof(Vehicle)
            );
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        inputDeps.Complete();

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        float deltaTime = Time.DeltaTime;
        NativeArray<Entity> producers = GetEntityQuery(ComponentType.ReadOnly<ResourceProducer>()).ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> consumers = GetEntityQuery(ComponentType.ReadOnly<ResourceConsumer>()).ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity e,ref Vehicle vehicle,ref Translation translation)=> {
            //grab the path buffer
            DynamicBuffer<Float3BufferElement> path = GetBufferFromEntity<Float3BufferElement>()[e];

            if (vehicle.IsActive && path.Length > 2) {

                //move vehicle towards next path position
                translation.Value = Vector3.MoveTowards(translation.Value, path[vehicle.ActivePathNode].Value,(vehicle.Speed * deltaTime));

                if(Vector3.Distance(translation.Value, path[vehicle.ActivePathNode].Value) < 0.25f) {
                    if (vehicle.IsMovingToProducer) {
                        if (vehicle.ActivePathNode == path.Length-1) {
                            // Vehicle has reached the producer
                            vehicle.IsMovingToProducer = false;
                            foreach (var item in producers) {
                                var producer = entityManager.GetComponentData<ResourceProducer>(item);
                                if (producer.ProducerId == vehicle.ProducerId) {
                                    //clean up expected out value now the vehicle has reached the producer
                                    var storage = entityManager.GetComponentData<ResourceStorage>(item);
                                    storage.ExpectedOut--;
                                    entityManager.SetComponentData(item, storage);
                                    break;
                                }
                            }
            
                        } else {
                            //next path position to producer
                            vehicle.ActivePathNode++;
                        }
                    } else {                 
                        if (vehicle.ActivePathNode == 0) {
                            //Vehicle reached the consumer
                            vehicle.IsMovingToProducer = true;
                            foreach (var item in consumers) {
                                var consumer = entityManager.GetComponentData<ResourceConsumer>(item);
                                if (consumer.ConsumerId == vehicle.VehicleId) {
                                    //Update consumers storage with new resource
                                    consumer.IsVehicleDispatched = false;
                                    var storage = entityManager.GetComponentData<ResourceStorage>(item);
                                    storage.Value++;
                                    vehicle.IsActive = false;
                                    entityManager.SetComponentData(item, consumer);
                                    break;
                                }
                            }
                        } else {
                            //next path position back to consumer
                            vehicle.ActivePathNode--;
                        }
                    }                 
                }
            }


        }).WithoutBurst().Run();


        producers.Dispose();
        consumers.Dispose();

        return inputDeps;
    }

    /// <summary>
    /// Run a A* pathfinding job on each vehicle to generate its paths and the cache them in a buffer
    /// </summary>
    /// <param name="gridWidth"></param>
    /// <param name="gridHeight"></param>
    internal void GeneratePaths(int gridWidth, int gridHeight) {
        EntityManager entityManager = EntityManager;
        NativeArray<Entity> producers = GetEntityQuery(ComponentType.ReadOnly<ResourceProducer>()).ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> consumers = GetEntityQuery(ComponentType.ReadOnly<ResourceConsumer>()).ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity e, ref Vehicle vehicle, ref Translation translation) => {

            WorldEntity consumer = entityManager.GetComponentData<WorldEntity>(consumers[vehicle.VehicleId]);
            WorldEntity producer = entityManager.GetComponentData<WorldEntity>(producers[vehicle.ProducerId]);

            NativeArray<Road> roads = GetEntityQuery(ComponentType.ReadOnly<Road>()).ToComponentDataArray<Road>(Allocator.TempJob);
            NativeList<float3> path = new NativeList<float3>(Allocator.TempJob);
            PathfindingJob job = new PathfindingJob {
                StartPosition = consumer.GridPosition,
                EndPosition = producer.GridPosition,
                Roads = roads,
                GridWidth = gridWidth,
                GridHeight = gridHeight,
                Result = path
            };


            JobHandle handle = job.Schedule();
            handle.Complete();

            DynamicBuffer<Float3BufferElement> pathCache = GetBufferFromEntity<Float3BufferElement>()[e];
            pathCache.Clear();
            for (int i = 0; i < job.Result.Length; i++) {
                pathCache.Add(new Float3BufferElement() { Value = new float3(job.Result[i]) });
            }

            path.Dispose();
            roads.Dispose();
            
        }).WithoutBurst().Run();

        producers.Dispose();
        consumers.Dispose();
    }

    //Spawn vehicle with default path positions, pathPositions[0] will be the starting position of the vehicle.
    public void SpawnVehicle(int id,Mesh mesh,Material material,float3[] pathPositions,int producerId) {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.CreateEntity(m_vehicleArchetype);
        entityManager.SetComponentData(entity, new Translation { Value = new float3 { x = (int)pathPositions[0].x, y = (int)pathPositions[0].y, z = -1 } });
        entityManager.SetComponentData(entity, new Vehicle { VehicleId = id, IsActive = false, IsMovingToProducer = true, ProducerId = producerId, Speed = 3 });
        entityManager.SetComponentData(entity, new WorldEntity { BlocksGrid = false, GridPosition = new float2 { x = (int)pathPositions[0].x, y = (int)pathPositions[0].y } });
        entityManager.SetSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material });
        DynamicBuffer<Float3BufferElement> path = entityManager.AddBuffer<Float3BufferElement>(entity);
        for (int i = 0; i < pathPositions.Length; i++) {
            path.Add(new Float3BufferElement() { Value = pathPositions[i] });
        }

    }

}
