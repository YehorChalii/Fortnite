using System.Collections.Generic;
using UnityEngine;

public class SnapPointsInitialization
{
    private Transform _floorSize;
    private Vector3 _cellSize;
    private int _YSnapPointsCount;

    private List<SnapPoint> _wallAllSP = new List<SnapPoint>();
    private List<SnapPoint> _centerAllSP = new List<SnapPoint>();
    private Dictionary<Vector3Int, SnapPoint> _snapPointsGrid = new Dictionary<Vector3Int, SnapPoint>();

    public SnapPointsInitialization(Transform floorSize, Vector3 cellSize, int YSnapPointsCount)
    {
        _floorSize = floorSize;
        _cellSize = cellSize;
        _YSnapPointsCount = YSnapPointsCount;
    }

    public (List<SnapPoint>, List<SnapPoint>, Dictionary<Vector3Int, SnapPoint>) GetSnapPoints()
    {
        InitializeSnapPoints();

        return (_wallAllSP, _centerAllSP, _snapPointsGrid);
    }

    private void InitializeSnapPoints()
    {
        Vector3 floorStartPosition = new Vector3(_floorSize.position.x, 0, _floorSize.position.z) - new Vector3(_floorSize.localScale.x / 2, 0, _floorSize.localScale.z / 2);
        float XSnapPointsCount = Mathf.RoundToInt(_floorSize.localScale.x / _cellSize.x);
        float ZSnapPointsCount = Mathf.RoundToInt(_floorSize.localScale.z / _cellSize.z);

        _centerAllSP = GetInitializedCenterSnapPoints(floorStartPosition, XSnapPointsCount, ZSnapPointsCount);
        _wallAllSP = GetInitializedWallSnapPoints(floorStartPosition, XSnapPointsCount, ZSnapPointsCount);
    }

    private List<SnapPoint> GetInitializedCenterSnapPoints(Vector3 floorStartPosition, float XSnapPointsCount, float ZSnapPointsCount)
    {
        List<SnapPoint> snapPoints = new List<SnapPoint>();

        for (int y = 0; y < _YSnapPointsCount; y++)
        {
            for (int x = 0; x < XSnapPointsCount; x++)
            {
                for (int z = 0; z < ZSnapPointsCount; z++)
                {
                    float XPosition = (_cellSize.x / 2) + (_cellSize.x * x);
                    float YPosition = (_cellSize.y / 5) + (_cellSize.y * y);
                    float ZPosition = (_cellSize.z / 2) + (_cellSize.z * z);

                    Vector3 snapPointPosition = floorStartPosition + new Vector3(XPosition, YPosition, ZPosition);
                    Vector3Int gridCoordinate = new Vector3Int(x * 2, y * 2, z * 2);

                    CenterSnapPoint centerSnapPoint = new CenterSnapPoint
                    {
                        Position = snapPointPosition,
                        GridCoordinate = gridCoordinate,
                        IsSupported = y == 0 ? true : false
                    };

                    snapPoints.Add(centerSnapPoint);

                    _snapPointsGrid.Add(gridCoordinate, centerSnapPoint);
                }
            }
        }

        Debug.Log($"Initialized Center Snap Points: {snapPoints.Count}");

        return snapPoints;
    }

    private List<SnapPoint> GetInitializedWallSnapPoints(Vector3 floorStartPosition, float XSnapPointsCount, float ZSnapPointsCount)
    {
        List<SnapPoint> snapPoints = new List<SnapPoint>();

        for (int y = 0; y < _YSnapPointsCount; y++)
        {
            for (int x = 0; x < XSnapPointsCount; x++)
            {
                for (int z = 0; z <= ZSnapPointsCount; z++)
                {
                    float XPosition = (_cellSize.x / 2) + (_cellSize.x * x);
                    float YPosition = (_cellSize.y * y);
                    float ZPosition = (_cellSize.z * z);

                    Vector3 snapPointPosition = floorStartPosition + new Vector3(XPosition, YPosition, ZPosition);
                    Vector3Int gridCoordinate = new Vector3Int(x * 2, y * 2, z * 2 - 1);

                    WallSnapPoint wallSnapPoint = new WallSnapPoint
                    {
                        Position = snapPointPosition,
                        Rotation = Quaternion.identity,
                        GridCoordinate = gridCoordinate,
                        IsSupported = y == 0 ? true : false
                    };

                    snapPoints.Add(wallSnapPoint);

                    _snapPointsGrid.Add(gridCoordinate, wallSnapPoint);
                }
            }
        }

        for (int y = 0; y < _YSnapPointsCount; y++)
        {
            for (int x = 0; x <= XSnapPointsCount; x++)
            {
                for (int z = 0; z < ZSnapPointsCount; z++)
                {
                    float XPosition = (_cellSize.x * x);
                    float YPosition = (_cellSize.y * y);
                    float ZPosition = (_cellSize.x / 2) + (_cellSize.z * z);

                    Vector3 snapPointPosition = floorStartPosition + new Vector3(XPosition, YPosition, ZPosition);
                    Vector3Int gridCoordinate = new Vector3Int(x * 2 - 1, y * 2, z * 2);

                    WallSnapPoint wallSnapPoint = new WallSnapPoint
                    {
                        Position = snapPointPosition,
                        Rotation = Quaternion.Euler(0, 90, 0),
                        GridCoordinate = gridCoordinate,
                        IsSupported = y == 0 ? true : false
                    };

                    snapPoints.Add(wallSnapPoint);

                    _snapPointsGrid.Add(gridCoordinate, wallSnapPoint);
                }
            }
        }

        Debug.Log($"Initialized Wall Snap Points: {snapPoints.Count}");

        return snapPoints;
    }
}