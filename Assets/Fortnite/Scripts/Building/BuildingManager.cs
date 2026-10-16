using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SelectedBuildingPiece 
{
    Wall,
    Ramp,
    Floor
}

public enum BuildingMaterial
{
    Wood,
    Brick,
    Metal
}

public class BuildingManager : MonoBehaviour
{
    public static Action<BuildingMaterial> OnBuildingPlaced;
    public static Action<BuildingMaterial> OnBuildingDestroyed;
    public static Action<bool> OnBuildingModeToggle;
    public static Action<bool> OnUpdateCrosshair;

    private bool _currentSnapPointAvailable;
    private bool _previousSnapPointAvailable;

    [Header("References")]
    [SerializeField] private CameraControl _cameraControl;
    [SerializeField] private Transform _character;

    [Space]
    [Header("Building Pieces")]
    [Header("Wood")]
    [SerializeField] private GameObject _woodWall;
    [SerializeField] private GameObject _woodRamp;
    [SerializeField] private GameObject _woodFloor;
    [Header("Brick")]
    [SerializeField] private GameObject _brickWall;
    [SerializeField] private GameObject _brickRamp;
    [SerializeField] private GameObject _brickFloor;
    [Header("Metal")]
    [SerializeField] private GameObject _metalWall;
    [SerializeField] private GameObject _metalRamp;
    [SerializeField] private GameObject _metalFloor;

    [Space]
    [Header("Preview Pieces")]
    [Header("Wood")]
    [SerializeField] private GameObject _woodWallPreview;
    [SerializeField] private GameObject _woodRampPreview;
    [SerializeField] private GameObject _woodFloorPreview;
    [Header("Brick")]
    [SerializeField] private GameObject _brickWallPreview;
    [SerializeField] private GameObject _brickRampPreview;
    [SerializeField] private GameObject _brickFloorPreview;
    [Header("Metal")]
    [SerializeField] private GameObject _metalWallPreview;
    [SerializeField] private GameObject _metalRampPreview;
    [SerializeField] private GameObject _metalFloorPreview;

    [Space]
    [Header("Properties")]
    [SerializeField] private float _placementDistance;
    [SerializeField] private float _minSnapDistance;
    [SerializeField] private float _destroyDistance;
    [SerializeField] private LayerMask _hitLayerMask;
    [SerializeField] private LayerMask _destroyLayerMask;

    [Space]
    [Header("Initialize Snap Points Properties")]
    [SerializeField] private SnapPointsInitialization _snapPointsInitialization;

    [Space]
    [Header("Optimization")]
    [SerializeField] private float _maxDistanceToUpdate;

    [Space]
    [Header("Gizmos")]
    [SerializeField] private bool _drawGizmos;
    [SerializeField] private bool _showInitializedSnapPoints;

    private PlayerInput _playerInput;

    private SelectedBuildingPiece _selectedBuildingPiece = SelectedBuildingPiece.Wall;

    private BuildingMaterial _selectedMaterial = BuildingMaterial.Wood;

    private bool _buildingMode = false;

    private List<SnapPoint> _wallAllSP = new List<SnapPoint>();
    private List<SnapPoint> _wallReachableSP = new List<SnapPoint>();
    private SnapPoint _wallClosestSP;

    private List<SnapPoint> _centerAllSP = new List<SnapPoint>();
    private List<SnapPoint> _centerReachableSP = new List<SnapPoint>();
    private SnapPoint _centerClosestSP;

    private SnapPoint _activeSnapPoint;

    private Dictionary<Vector3Int, SnapPoint> _snapPointsGrid = new Dictionary<Vector3Int, SnapPoint>();

    private float _currentDistance;
    private Vector3 _previousPosition;

    private GameObject _previewInstance;
    private GameObject _previewSourcePrefab;

    private Quaternion _currentBuildingPieceRotation;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        _playerInput.Player.ToggleBuildingMode.performed += HandleToggleBuildingMode;
        _playerInput.Player.SelectWall.performed += HandleSelectWall;
        _playerInput.Player.SelectRamp.performed += HandleSelectRamp;
        _playerInput.Player.SelectFloor.performed += HandleSelectFloor;
        _playerInput.Player.Place.performed += HandlePlace;
        _playerInput.Player.Destroy.performed += HandleDestroy;
        _playerInput.Player.SwitchMaterial.performed += HandleSwitchMaterial;
        _playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.ToggleBuildingMode.performed -= HandleToggleBuildingMode;
        _playerInput.Player.SelectWall.performed -= HandleSelectWall;
        _playerInput.Player.SelectRamp.performed -= HandleSelectRamp;
        _playerInput.Player.SelectFloor.performed -= HandleSelectFloor;
        _playerInput.Player.Place.performed -= HandlePlace;
        _playerInput.Player.Destroy.performed -= HandleDestroy;
        _playerInput.Player.SwitchMaterial.performed -= HandleSwitchMaterial;
        _playerInput.Player.Disable();
    }

    private void HandleToggleBuildingMode(InputAction.CallbackContext ctx)
    {
        _buildingMode = !_buildingMode;

        if (_buildingMode)
        {
            if (_selectedBuildingPiece == SelectedBuildingPiece.Wall)
            {
                UpdateSnapPointList(_wallReachableSP, _wallAllSP);
            }
            else
            {
                UpdateSnapPointList(_centerReachableSP, _centerAllSP);
            }

            _previousPosition = _character.transform.position;

            OnUpdateCrosshair?.Invoke(_activeSnapPoint != null);
        }
        else
        {
            DestroyPreview();
        }

        OnBuildingModeToggle?.Invoke(_buildingMode);

        Debug.Log($"Building mode: {_buildingMode}");
    }

    private void HandleSelectWall(InputAction.CallbackContext ctx) => TrySwitchBuildingPiece(SelectedBuildingPiece.Wall);

    private void HandleSelectRamp(InputAction.CallbackContext ctx) => TrySwitchBuildingPiece(SelectedBuildingPiece.Ramp);

    private void HandleSelectFloor(InputAction.CallbackContext ctx) => TrySwitchBuildingPiece(SelectedBuildingPiece.Floor);

    private void HandleSwitchMaterial(InputAction.CallbackContext ctx)
    {
        if (!_buildingMode) return;

        int count = Enum.GetValues(typeof(BuildingMaterial)).Length;
        _selectedMaterial = (BuildingMaterial)(((int)_selectedMaterial + 1) % count);

        DestroyPreview();

        Debug.Log($"Selected material: {_selectedMaterial}");
    }

    private void TrySwitchBuildingPiece(SelectedBuildingPiece buildingPiece)
    {
        if (!_buildingMode) return;

        if (_selectedBuildingPiece == buildingPiece)
        {
            Debug.Log($"Selected piece: {_selectedBuildingPiece}");
            return;
        }

        _selectedBuildingPiece = buildingPiece;

        if (_selectedBuildingPiece == SelectedBuildingPiece.Wall)
        {
            UpdateSnapPointList(_wallReachableSP, _wallAllSP);
        }
        else
        {
            UpdateSnapPointList(_centerReachableSP, _centerAllSP);
        }

        _previousPosition = _character.transform.position;

        DestroyPreview();

        Debug.Log($"Selected piece: {_selectedBuildingPiece}");
    }

    private void HandlePlace(InputAction.CallbackContext ctx)
    {
        if (!_buildingMode) return;

        GameObject currentBuildingPiece = GetCurrentBuildingPiece();

        if (_selectedBuildingPiece == SelectedBuildingPiece.Wall)
        {
            if (_wallClosestSP != null)
            {
                PlaceBuildingPiece(currentBuildingPiece, _currentBuildingPieceRotation, _wallClosestSP, _selectedBuildingPiece, _selectedMaterial);
            }
        }
        else
        {
            if (_centerClosestSP != null)
            {
                PlaceBuildingPiece(currentBuildingPiece, _currentBuildingPieceRotation, _centerClosestSP, _selectedBuildingPiece, _selectedMaterial);
            }
        }
    }

    private void PlaceBuildingPiece(GameObject buildingPiece, Quaternion rotation, SnapPoint closestSnapPoint, SelectedBuildingPiece selectedBuildingPiece, BuildingMaterial buildingMaterial)
    {
        GameObject instantiatedBuildingPiece = Instantiate(buildingPiece, closestSnapPoint.Position, rotation);
        if (instantiatedBuildingPiece.TryGetComponent<BuildingPiece>(out var buildingPieceComponent))
        {
            buildingPieceComponent.AssignSnapPoint(closestSnapPoint, selectedBuildingPiece, buildingMaterial);
        }

        closestSnapPoint.TakeSlot(selectedBuildingPiece);

        SupportNeighboringSnapPoints(selectedBuildingPiece, rotation);

        if (selectedBuildingPiece == SelectedBuildingPiece.Wall)
        {
            UpdateSnapPointList(_wallReachableSP, _wallAllSP);
        }
        else
        {
            UpdateSnapPointList(_centerReachableSP, _centerAllSP);
        }

        DestroyPreview();
        OnBuildingPlaced?.Invoke(buildingMaterial);
        Debug.Log($"Placed: {buildingPiece.name}");
    }

    private void HandleDestroy(InputAction.CallbackContext ctx)
    {
        if (_buildingMode) return;

        Ray ray = new Ray(_cameraControl.transform.position, _cameraControl.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _destroyDistance, _destroyLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.TryGetComponent<BuildingPiece>(out var buildingPieceComponent))
            {
                buildingPieceComponent.SnapPoint.FreeSlot(buildingPieceComponent.BuildingPieceType);

                OnBuildingDestroyed?.Invoke(buildingPieceComponent.BuildingMaterial);

                Destroy(hitObject);

                Debug.Log($"Destroyed: {hitObject.name}");
            }
        }
    }

    private void Start()
    {
        SnapPointsData snapPoints = _snapPointsInitialization.GetSnapPoints();

        _wallAllSP = snapPoints.WallSnapPoints;
        _centerAllSP = snapPoints.CenterSnapPoints;
        _snapPointsGrid = snapPoints.SnapPointsGrid;

        OnBuildingModeToggle?.Invoke(_buildingMode);
    }

    private void Update()
    {
        UpdateSnapPoints();
        SetClosestSnapPoint();
        UpdateUI();
    }

    private void UpdateSnapPoints()
    {
        if (!_buildingMode) return;

        _currentDistance = Vector3.Distance(_character.transform.position, _previousPosition);

        if (_currentDistance >= _maxDistanceToUpdate)
        {
            if (_selectedBuildingPiece == SelectedBuildingPiece.Wall)
            {
                UpdateSnapPointList(_wallReachableSP, _wallAllSP);
            }
            else
            {
                UpdateSnapPointList(_centerReachableSP, _centerAllSP);
            }

            _previousPosition = _character.transform.position;
        }
    }
    private void UpdateSnapPointList(List<SnapPoint> reachableSnapPoints, List<SnapPoint> allSnapPoints)
    {
        reachableSnapPoints.Clear();

        foreach (var snapPoint in allSnapPoints)
        {
            if (!snapPoint.IsAvailable(_selectedBuildingPiece) ||
                !snapPoint.IsSupported) continue;

            float currentDistance = Vector3.Distance(snapPoint.Position, _character.position);
            if (currentDistance <= _placementDistance)
            {
                reachableSnapPoints.Add(snapPoint);
            }
        }
    }

    private void SetClosestSnapPoint()
    {
        if (!_buildingMode) return;

        if (_selectedBuildingPiece == SelectedBuildingPiece.Wall)
        {
            _wallClosestSP = GetClosestSnapPoint(_wallReachableSP);
            _activeSnapPoint = _wallClosestSP;
        }
        else
        {
            _centerClosestSP = GetClosestSnapPoint(_centerReachableSP);
            _activeSnapPoint = _centerClosestSP;
        }

        _previousSnapPointAvailable = _currentSnapPointAvailable;
        _currentSnapPointAvailable = _activeSnapPoint != null ? true : false;
    }

    private SnapPoint GetClosestSnapPoint(List<SnapPoint> reachableSnapPoints)
    {
        Vector3 rayPoint;
        Ray ray = new Ray(_cameraControl.transform.position, _cameraControl.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _placementDistance, _hitLayerMask))
        {
            rayPoint = hit.point;
        }
        else
        {
            rayPoint = ray.GetPoint(_placementDistance);
        }

        SnapPoint closestSnapPoint = null;
        float smallestDistance = _minSnapDistance;

        foreach (var snapPoint in reachableSnapPoints)
        {
            if (!snapPoint.IsAvailable(_selectedBuildingPiece) ||
                !snapPoint.IsSupported) continue;

            float currentDistance = Vector3.Distance(snapPoint.Position, rayPoint);
            if (currentDistance < smallestDistance)
            {
                smallestDistance = currentDistance;
                closestSnapPoint = snapPoint;
            }
        }

        return closestSnapPoint;
    }

    private void UpdateUI()
    {
        if (!_buildingMode) return;
        UpdatePreview();
        UpdateCrossHair();
    }

    private void UpdatePreview()
    {
        if (!_currentSnapPointAvailable)
        {
            DestroyPreview();
            return;
        }

        _currentBuildingPieceRotation = _selectedBuildingPiece == SelectedBuildingPiece.Wall
            ? _wallClosestSP.Rotation
            : GetBuildingPieceAngle(_cameraControl.GetCameraFlatForward());

        GameObject currentPreview = GetCurrentPreview();

        if (_previewInstance == null || _previewSourcePrefab != currentPreview)
        {
            DestroyPreview();
            SpawnPreview(currentPreview, _activeSnapPoint.Position, _currentBuildingPieceRotation);
            return;
        }

        _previewInstance.transform.SetPositionAndRotation(_activeSnapPoint.Position, _currentBuildingPieceRotation);
    }

    private void SpawnPreview(GameObject previewPrefab, Vector3 position, Quaternion rotation)
    {
        _previewSourcePrefab = previewPrefab;
        _previewInstance = Instantiate(previewPrefab, position, rotation);
    }

    private void DestroyPreview()
    {
        if (_previewInstance != null)
        {
            Destroy(_previewInstance);
            _previewInstance = null;
        }

        _previewSourcePrefab = null;
    }

    private void UpdateCrossHair()
    {
        if (_previousSnapPointAvailable != _currentSnapPointAvailable)
        {
            OnUpdateCrosshair?.Invoke(_activeSnapPoint != null);
        }
    }

    private void SupportNeighboringSnapPoints(SelectedBuildingPiece buildingPiece, Quaternion rotation)
    {
        switch (buildingPiece)
        {
            case SelectedBuildingPiece.Wall:
                HandleWallSupport(rotation);
                break;
            case SelectedBuildingPiece.Ramp:
                HandleRampSupport(rotation);
                break;
            case SelectedBuildingPiece.Floor:
                HandleFloorSupport();
                break;
        }
    }

    private void HandleWallSupport(Quaternion rotation)
    {
        //Walls Support
        SupportSnapPoint(new Vector3Int(0, 2, 0), _wallClosestSP); //up
        SupportSnapPoint(new Vector3Int(0, -2, 0), _wallClosestSP); //down
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(-2, 0, 0)), _wallClosestSP); //left relative
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(2, 0, 0)), _wallClosestSP); //right relative

        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(-1, 0, 1)), _wallClosestSP);
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(1, 0, 1)), _wallClosestSP);
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(-1, 0, -1)), _wallClosestSP);
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(1, 0, -1)), _wallClosestSP);

        //Center Support
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 0, 1)), _wallClosestSP); //front
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 0, -1)), _wallClosestSP); //back
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 2, 1)), _wallClosestSP); //front up
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 2, -1)), _wallClosestSP); //back up
    }

    private void HandleRampSupport(Quaternion rotation)
    {
        //Center Support
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(-2, 0, 0)), _centerClosestSP); //left
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(2, 0, 0)), _centerClosestSP); //right
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 0, -2)), _centerClosestSP); //back
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 2, 2)), _centerClosestSP); //front up

        //Wall Support
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 0, 1)), _centerClosestSP); //front
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(1, 0, 0)), _centerClosestSP); //back
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(-1, 0, 0)), _centerClosestSP); //left
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 0, -1)), _centerClosestSP); //right
        SupportSnapPoint(GetRotationRelativeOffset(rotation, new Vector3Int(0, 2, 1)), _centerClosestSP); //front up
    }

    private void HandleFloorSupport()
    {
        //Center Support
        SupportSnapPoint(new Vector3Int(2, 0, 0), _centerClosestSP);
        SupportSnapPoint(new Vector3Int(-2, 0, 0), _centerClosestSP);
        SupportSnapPoint(new Vector3Int(0, 0, 2), _centerClosestSP);
        SupportSnapPoint(new Vector3Int(0, 0, -2), _centerClosestSP);

        //Wall Support
        SupportSnapPoint(new Vector3Int(1, 0, 0), _centerClosestSP);
        SupportSnapPoint(new Vector3Int(-1, 0, 0), _centerClosestSP);
        SupportSnapPoint(new Vector3Int(0, 0, 1), _centerClosestSP);
        SupportSnapPoint(new Vector3Int(0, 0, -1), _centerClosestSP);
    }

    private void SupportSnapPoint(Vector3Int offset, SnapPoint closestSnapPoint)
    {
        Vector3Int neighboringGridCoordinate = closestSnapPoint.GridCoordinate + offset;
        if (_snapPointsGrid.TryGetValue(neighboringGridCoordinate, out SnapPoint snapPoint))
        {
            snapPoint.IsSupported = true;
        }
    }

    private Vector3Int GetRotationRelativeOffset(Quaternion rotation, Vector3Int offset)
    {
        Vector3 rotated = rotation * (Vector3)offset;
        return new Vector3Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y), Mathf.RoundToInt(rotated.z));
    }

    private Quaternion GetBuildingPieceAngle(Vector3 cameraForward)
    {
        if (Vector3.Dot(cameraForward, Vector3.forward) > 0.707f) return Quaternion.Euler(0, 0, 0);
        if (Vector3.Dot(cameraForward, Vector3.right) > 0.707f) return Quaternion.Euler(0, 90, 0);
        if (Vector3.Dot(cameraForward, Vector3.back) > 0.707f) return Quaternion.Euler(0, 180, 0);
        if (Vector3.Dot(cameraForward, Vector3.left) > 0.707f) return Quaternion.Euler(0, 270, 0);

        else return Quaternion.identity;
    }

    private GameObject GetCurrentBuildingPiece()
    {
        return (_selectedMaterial, _selectedBuildingPiece) switch
        {
            (BuildingMaterial.Wood, SelectedBuildingPiece.Wall) => _woodWall,
            (BuildingMaterial.Wood, SelectedBuildingPiece.Ramp) => _woodRamp,
            (BuildingMaterial.Wood, SelectedBuildingPiece.Floor) => _woodFloor,
            (BuildingMaterial.Brick, SelectedBuildingPiece.Wall) => _brickWall,
            (BuildingMaterial.Brick, SelectedBuildingPiece.Ramp) => _brickRamp,
            (BuildingMaterial.Brick, SelectedBuildingPiece.Floor) => _brickFloor,
            (BuildingMaterial.Metal, SelectedBuildingPiece.Wall) => _metalWall,
            (BuildingMaterial.Metal, SelectedBuildingPiece.Ramp) => _metalRamp,
            (BuildingMaterial.Metal, SelectedBuildingPiece.Floor) => _metalFloor,
            _ => _woodWall
        };
    }

    private GameObject GetCurrentPreview()
    {
        return (_selectedMaterial, _selectedBuildingPiece) switch
        {
            (BuildingMaterial.Wood, SelectedBuildingPiece.Wall) => _woodWallPreview,
            (BuildingMaterial.Wood, SelectedBuildingPiece.Ramp) => _woodRampPreview,
            (BuildingMaterial.Wood, SelectedBuildingPiece.Floor) => _woodFloorPreview,
            (BuildingMaterial.Brick, SelectedBuildingPiece.Wall) => _brickWallPreview,
            (BuildingMaterial.Brick, SelectedBuildingPiece.Ramp) => _brickRampPreview,
            (BuildingMaterial.Brick, SelectedBuildingPiece.Floor) => _brickFloorPreview,
            (BuildingMaterial.Metal, SelectedBuildingPiece.Wall) => _metalWallPreview,
            (BuildingMaterial.Metal, SelectedBuildingPiece.Ramp) => _metalRampPreview,
            (BuildingMaterial.Metal, SelectedBuildingPiece.Floor) => _metalFloorPreview,
            _ => _woodWallPreview
        };
    }

    private void OnDrawGizmos()
    {
        if (_showInitializedSnapPoints)
        {
            DrawInitializedSnapPoints(_wallAllSP, Color.green, new Vector3(0.2f, 0.2f, 0.2f));
            DrawInitializedSnapPoints(_centerAllSP, Color.yellow, new Vector3(0.2f, 0.2f, 0.2f));
        }

        if (!_drawGizmos || !_buildingMode) return;

        if (_selectedBuildingPiece == SelectedBuildingPiece.Wall)
        {
            DrawReachableSnapPoints(_wallReachableSP, _wallClosestSP);
        }
        else
        {
            DrawReachableSnapPoints(_centerReachableSP, _centerClosestSP);
        }
    }

    private void DrawInitializedSnapPoints(List<SnapPoint> snapPoints, Color color, Vector3 size)
    {
        if (snapPoints != null && snapPoints.Count > 0)
        {
            Gizmos.color = color;

            foreach (var snapPoint in snapPoints)
            {
                Gizmos.DrawWireCube(snapPoint.Position, size);
            }
        }
    }

    private void DrawReachableSnapPoints(List<SnapPoint> reachableSnapPoints, SnapPoint closestSnapPoint)
    {
        DrawSnapPoints(reachableSnapPoints, Color.cyan, new Vector3(0.3f, 0.3f, 0.3f));

        if (closestSnapPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(closestSnapPoint.Position, new Vector3(0.4f, 0.4f, 0.4f));
        }
    }

    private void DrawSnapPoints(List<SnapPoint> snapPoints, Color color, Vector3 size)
    {
        if (snapPoints == null || snapPoints.Count == 0) return;

        Gizmos.color = color;
        foreach (var snapPoint in snapPoints)
        {
            if (!snapPoint.IsAvailable(_selectedBuildingPiece) ||
                !snapPoint.IsSupported) continue;

            Gizmos.DrawWireCube(snapPoint.Position, size);
        }
    }
}