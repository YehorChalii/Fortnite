using System.Collections.Generic;
using UnityEngine;

public class ReachableSnapPoints
{
    private List<SnapPoint> _wallReachableSP = new List<SnapPoint>();
    public List<SnapPoint> _centerReachableSP = new List<SnapPoint>();

    private List<SnapPoint> _wallAllSP = new List<SnapPoint>();
    private List<SnapPoint> _centerAllSP = new List<SnapPoint>();

    private Transform _playerTransform;
    private float _placementDistance;

    public ReachableSnapPoints(List<SnapPoint> wallSP, List<SnapPoint> centerSP, Transform playerTransform, float placementDistance)
    {
        _wallAllSP = wallSP;
        _centerAllSP = centerSP;
        _playerTransform = playerTransform;
        _placementDistance = placementDistance;
    }

    public List<SnapPoint> GetSnapPoints(SelectedBuildingPiece selectedBP)
    {
        if (selectedBP == SelectedBuildingPiece.Wall)
        {
            return _wallReachableSP;
        }
        else
        {
            return _centerReachableSP;
        }
    }

    public void UpdateSnapPoints(SelectedBuildingPiece selectedBP)
    {
        if (selectedBP == SelectedBuildingPiece.Wall)
        {
            UpdateSnapPointList(_wallReachableSP, _wallAllSP, selectedBP);
        }
        else
        {
            UpdateSnapPointList(_centerReachableSP, _centerAllSP, selectedBP);
        }
    }

    private void UpdateSnapPointList(List<SnapPoint> reachableSnapPoints, List<SnapPoint> allSnapPoints, SelectedBuildingPiece selectedBP)
    {
        foreach (var snapPoint in allSnapPoints)
        {
            if (!snapPoint.IsAvailable(selectedBP) ||
                !snapPoint.IsSupported) continue;

            float currentDistance = Vector3.Distance(snapPoint.Position, _playerTransform.position);

            if (currentDistance <= _placementDistance)
            {
                reachableSnapPoints.Add(snapPoint);
            }
        }
    }
}