using System.Collections.Generic;
using UnityEngine;

public class SnapPointsInitialization : MonoBehaviour
{
    [SerializeField] private Transform _floorTransform;
    [SerializeField] private Vector3 _cellSize;
    [SerializeField] private int _yCount;

    private Vector3 _startPosition;
    private Vector3Int _count;

    private List<SnapPoint> _wallSP = new List<SnapPoint>();
    private List<SnapPoint> _centerSP = new List<SnapPoint>();
    private Dictionary<Vector3Int, SnapPoint> _snapPointsGrid = new Dictionary<Vector3Int, SnapPoint>();

    private void Awake()
    {
        _startPosition = GetStartPosition();
        _count = GetCount();
    }

    private Vector3 GetStartPosition()
    {
        return new Vector3(_floorTransform.position.x - _floorTransform.localScale.x / 2f, 0f, _floorTransform.position.z - _floorTransform.localScale.z / 2f);
    }

    private Vector3Int GetCount()
    {
        int xCount = Mathf.RoundToInt(_floorTransform.localScale.x / _cellSize.x);
        int zCount = Mathf.RoundToInt(_floorTransform.localScale.z / _cellSize.z);

        return new Vector3Int(xCount, _yCount, zCount);
    }

    //

    public SnapPointsData GetSnapPoints()
    {
        InitializeSnapPoints();

        return new SnapPointsData(_wallSP, _centerSP, _snapPointsGrid);
    }

    private void InitializeSnapPoints()
    {
        InitializeCenterSnapPoints();
        InitializeWallSnapPoints();

        Debug.Log($"Initialized {_centerSP.Count} center snap points and {_wallSP.Count} wall snap points.");
    }

    private void InitializeCenterSnapPoints()
    {
        Vector3 offset = new Vector3(_cellSize.x / 2f, 1f, _cellSize.z / 2f);

        _centerSP = GetInitializedSnapPoints<CenterSnapPoint>(_count, offset, Quaternion.identity, Vector3Int.zero);
    }

    private void InitializeWallSnapPoints()
    {
        //Along X
        Vector3Int count = new Vector3Int(_count.x, _count.y, _count.z + 1);
        Vector3 offset = new Vector3(_cellSize.x / 2f, 0f, 0f);

        _wallSP = GetInitializedSnapPoints<WallSnapPoint>(count, offset, Quaternion.identity, new Vector3Int(0, 0, -1));

        //ALong Z
        count = new Vector3Int(_count.x + 1, _count.y, _count.z);
        offset = new Vector3(0f, 0f, _cellSize.z / 2f);
        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

        _wallSP.AddRange(GetInitializedSnapPoints<WallSnapPoint>(count, offset, rotation, new Vector3Int(-1, 0, 0)));
    }

    private List<SnapPoint> GetInitializedSnapPoints<T>(
        Vector3Int count,
        Vector3 offset,
        Quaternion rotation,
        Vector3Int gridOffset) where T : SnapPoint, new()
    {
        List<SnapPoint> snapPoints = new List<SnapPoint>();
        List<Vector3> positions = GetSnapPointsPositionsList(count, offset);

        bool hasGridOffset = gridOffset != Vector3Int.zero;

        int index = 0;

        for (int x = 0; x < count.x; x++)
        {
            for (int y = 0; y < count.y; y++)
            {
                for (int z = 0; z < count.z; z++)
                {
                    Vector3Int gridCoordinate = new Vector3Int(
                        x * 2 + (hasGridOffset ? 0 : gridOffset.x),
                        y * 2 + gridOffset.y,
                        z * 2 + (hasGridOffset ? 0 : gridOffset.z)
                    );

                    T snapPoint = new T
                    {
                        Position = positions[index++],
                        GridCoordinate = gridCoordinate,
                        IsSupported = y == 0
                    };

                    if (snapPoint is WallSnapPoint wallSnapPoint)
                    {
                        wallSnapPoint.Rotation = rotation;
                    }

                    snapPoints.Add(snapPoint);
                    _snapPointsGrid.Add(snapPoint.GridCoordinate, snapPoint);
                }
            }
        }

        return snapPoints;
    }

    private List<Vector3> GetSnapPointsPositionsList(Vector3Int count, Vector3 offset)
    {
        List<Vector3> positions = new List<Vector3>();

        for (int x = 0; x < count.x; x++)
        {
            for (int y = 0; y < count.y; y++)
            {
                for (int z = 0; z < count.z; z++)
                {
                    Vector3 position = new Vector3(_cellSize.x * x, _cellSize.y * y, _cellSize.z * z);
                    positions.Add(_startPosition + position + offset);
                }
            }
        }

        return positions;
    }
}