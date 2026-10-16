using System.Collections.Generic;
using UnityEngine;

public class SnapPointsData
{
    public List<SnapPoint> WallSnapPoints { get; }
    public List<SnapPoint> CenterSnapPoints { get; }
    public Dictionary<Vector3Int, SnapPoint> SnapPointsGrid { get; }

    public SnapPointsData(List<SnapPoint> wallSnapPoints, List<SnapPoint> centerSnapPoints, Dictionary<Vector3Int, SnapPoint> snapPointsGrid)
    {
        WallSnapPoints = wallSnapPoints;
        CenterSnapPoints = centerSnapPoints;
        SnapPointsGrid = snapPointsGrid;
    }
}