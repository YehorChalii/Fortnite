using System.Collections.Generic;
using UnityEngine;

public class SnapPointsInitializer : MonoBehaviour
{
    [SerializeField] private Transform floorTransform;
    [SerializeField] private Vector3 cellSize;

    [Range(1f, 10f)]
    [SerializeField] private int yCount;
    [SerializeField] private float yPositionOffset;

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
        return new Vector3(floorTransform.position.x - floorTransform.localScale.x / 2f, 0f, floorTransform.position.z - floorTransform.localScale.z / 2f);
    }

    private Vector3Int GetCount()
    {
        int xCount = Mathf.RoundToInt(floorTransform.localScale.x / cellSize.x);
        int zCount = Mathf.RoundToInt(floorTransform.localScale.z / cellSize.z);

        return new Vector3Int(xCount, yCount, zCount);
    }

    //

    public SnapPointsData GetSnapPoints()
    {
        InitializeSnapPoints();

        return new SnapPointsData(_wallSP, _centerSP, _snapPointsGrid);
    }

    private void InitializeSnapPoints()
    {
        InitializeWallSnapPoints();
        InitializeCenterSnapPoints();

        Debug.Log($"Initialized {_centerSP.Count} center snap points and {_wallSP.Count} wall snap points.");
    }

    private void InitializeWallSnapPoints()
    {
        //Along X
        Vector3Int count = new Vector3Int(_count.x, _count.y, _count.z + 1);
        Vector3 offset = new Vector3(cellSize.x / 2f, 0f, 0f);

        _wallSP = GetInitializedSnapPointsList<WallSnapPoint>(count, offset, Quaternion.identity, new Vector3Int(0, 0, -1));

        //ALong Z
        count = new Vector3Int(_count.x + 1, _count.y, _count.z);
        offset = new Vector3(0f, 0f, cellSize.z / 2f);
        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

        _wallSP.AddRange(GetInitializedSnapPointsList<WallSnapPoint>(count, offset, rotation, new Vector3Int(-1, 0, 0)));
    }

    private void InitializeCenterSnapPoints()
    {
        Vector3 offset = new Vector3(cellSize.x / 2f, yPositionOffset, cellSize.z / 2f);

        _centerSP = GetInitializedSnapPointsList<CenterSnapPoint>(_count, offset, Quaternion.identity, Vector3Int.zero);
    }

    private List<SnapPoint> GetInitializedSnapPointsList<T>(
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
                        x * 2 + (hasGridOffset ? gridOffset.x : 0),
                        y * 2 + gridOffset.y,
                        z * 2 + (hasGridOffset ? gridOffset.z : 0)
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
                    _snapPointsGrid.Add(gridCoordinate, snapPoint);
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
                    Vector3 position = new Vector3(cellSize.x * x, cellSize.y * y, cellSize.z * z);
                    positions.Add(_startPosition + position + offset);
                }
            }
        }

        return positions;
    }
}