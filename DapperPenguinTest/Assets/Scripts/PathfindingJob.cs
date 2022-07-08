using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct PathfindingJob : IJob {

    /// <summary>
    /// Pretty simple A* pathfinding job used once for each vehicle then path is cached since it will never change.
    /// Marking nodes as traverable using roads, this could have been done better.
    /// </summary>

    public int GridWidth;
    public int GridHeight;
    public float2 StartPosition;
    public float2 EndPosition;
    public NativeArray<Road> Roads;
    public NativeList<float3> Result;

    public void Execute() {
        NativeArray<PathNode> pathNodes = new NativeArray<PathNode>(GridWidth * GridHeight, Allocator.Temp);

        //Generate pathnodes
        for (int y = 0; y < GridHeight; y++) {
            for (int x = 0; x < GridWidth; x++) {
                PathNode pathNode = new PathNode() {
                    Position = new int2 { x = x, y = y },
                    Index = GetIndexFromPosition(new float2(x, y)),
                    GroundCost = int.MaxValue,
                    Heuristic = (int)Vector2.Distance(new float2 { x = x, y = y }, EndPosition),
                    IsTraverseable = false,
                    PreviousNodeIndex = -1
                };

                for (int i = 0; i < Roads.Length; i++) {
                    if (Roads[i].GridPosition.Equals(pathNode.Position))
                        pathNode.IsTraverseable = true;
                }

                pathNode.CalculateCost();
                pathNodes[pathNode.Index] = pathNode;
            }
        }

        NativeList<int> openSet = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedSet = new NativeList<int>(Allocator.Temp);

        //setup start node
        PathNode startNode = pathNodes[GetIndexFromPosition(StartPosition)];
        startNode.GroundCost = 0;
        startNode.CalculateCost();

        //Make sure start node is traversable as its always a building
        startNode.IsTraverseable = true;
        pathNodes[startNode.Index] = startNode;

        openSet.Add(startNode.Index);
        int endNodeIndex = GetIndexFromPosition(EndPosition);
        PathNode endNode = pathNodes[endNodeIndex];

        //Make sure end node is traversable as its always a building
        endNode.IsTraverseable = true;
        pathNodes[endNodeIndex] = endNode;


        //cached vector offsets for grid positions
        NativeArray<float2> neighbourOffsetArray = new NativeArray<float2>(4, Allocator.Temp);
        neighbourOffsetArray[0] = new float2(0, 1);
        neighbourOffsetArray[1] = new float2(0, -1);
        neighbourOffsetArray[2] = new float2(-1, 0);
        neighbourOffsetArray[3] = new float2(1, 0);

        //Actually start finding the path
        while (openSet.Length > 0) {
            int currentNodeIndex = GetLowestCostNode(openSet, pathNodes);
            PathNode currentNode = pathNodes[currentNodeIndex];

            //Reached the end
            if (currentNodeIndex == endNodeIndex) {
                break;
            }

            for (int i = 0; i < openSet.Length; i++) {
                if (openSet[i] == currentNodeIndex) {
                    openSet.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedSet.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++) {
                //Check the position is actually inside the grid
                float2 neighbourPosition = currentNode.Position + neighbourOffsetArray[i];
                if (!IsValidPosition(neighbourPosition)) {
                    continue;
                }

                int neighbourIndex = GetIndexFromPosition(neighbourPosition);
                if (openSet.Contains(neighbourIndex)) {
                    continue;
                }

                if (!pathNodes[neighbourIndex].IsTraverseable) {
                    continue;
                }

                // Check costs and update openset acordingly
                int cost = (int)(currentNode.GroundCost + Vector2.Distance(currentNode.Position, pathNodes[neighbourIndex].Position));
                if (cost < pathNodes[neighbourIndex].GroundCost) {
                    PathNode neighbourNode = pathNodes[neighbourIndex];
                    neighbourNode.PreviousNodeIndex = currentNodeIndex;
                    neighbourNode.GroundCost = cost;
                    neighbourNode.CalculateCost();
                    pathNodes[neighbourIndex] = neighbourNode;

                    if (!openSet.Contains(neighbourNode.Index)) {
                        openSet.Add(neighbourNode.Index);
                    }
                }
            }
        }

        // end node is valid now calculate the result
        if (pathNodes[endNodeIndex].PreviousNodeIndex != -1) {
            CalculatePath(pathNodes[endNodeIndex], pathNodes);
        }

        openSet.Dispose();
        closedSet.Dispose();
        pathNodes.Dispose();
        neighbourOffsetArray.Dispose();
    }

    private void CalculatePath(PathNode endNode, NativeArray<PathNode> pathNodes) {
        NativeList<float3> path = new NativeList<float3>(Allocator.Temp);
        path.Add(new float3(endNode.Position.x,endNode.Position.y,0));

        PathNode currentNode = endNode;
        while (currentNode.PreviousNodeIndex != -1) {
            PathNode previousNode = pathNodes[currentNode.PreviousNodeIndex];
            path.Add(new float3(previousNode.Position.x,previousNode.Position.y,0));
            currentNode = previousNode;
        }

        //reverse the path
        for (int i = 0; i < path.Length; i++) {
            Result.Add(new float3 { x  = path[(path.Length -1)- i].x, y = path[(path.Length-1) - i].y, z = -1 });
        }

        path.Dispose();
    }

    private int GetIndexFromPosition(float2 position) {
        return (int)position.x + (int)position.y * GridWidth;
    }

    private bool IsValidPosition(float2 positon) {
        return (positon.x >= 0 &&
                positon.x < GridWidth &&
                positon.y < GridHeight &&
                positon.y >= 0);
    }

    private int GetLowestCostNode(NativeList<int> openSet, NativeArray<PathNode> pathNodes) {
        PathNode node = pathNodes[openSet[0]];
        for (int i = 0; i < openSet.Length; i++) {
            if (pathNodes[openSet[i]].Cost < node.Cost)
                node = pathNodes[openSet[i]];
        }
        return node.Index;
    }
}
