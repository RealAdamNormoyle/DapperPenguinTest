using Unity.Mathematics;


/// <summary>
/// Pathnode data struct used for pathfinding.
/// </summary>
public struct PathNode
{
    public float2 Position;
    public int Index;
    public int GroundCost;
    public int Heuristic;
    public int Cost;
    public bool IsTraverseable;
    public int PreviousNodeIndex;

    public void CalculateCost() {
        Cost = Heuristic + GroundCost;
    }

}
