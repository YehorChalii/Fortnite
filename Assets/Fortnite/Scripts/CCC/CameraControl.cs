using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _target;

    [Space]
    [Header("Properties")]
    [SerializeField] private float _distance;
    [SerializeField] private float _height;
    [SerializeField] private Vector3 _offset;

    [Space]
    [Header("Rotation Speed")]
    [SerializeField] private float _horizontalRotationSpeed;
    [SerializeField] private float _verticalRotationSpeed;

    [Header("Limits")]
    [SerializeField, Range(-90f, 90f)] private float _lowestVerticalLimit;
    [SerializeField, Range(-90f, 90f)] private float _highestVerticalLimit;
    
    [Space]
    [Header("Collisions")]
    [SerializeField] private float _sphereCastCollisionRadius;
    [SerializeField] private LayerMask _collisionLayerMask;

    [Header("Visibility")]
    [SerializeField] private GameObject _characterBody;
    [SerializeField, Range(0f, 0.5f)] private float _visibilityThreshold;

    private PlayerInput _playerInput;
    private Vector2 _inputVector;
    private float _horizontalRotation;
    private float _verticalRotation;

    private Vector3 _rawPosition;
    private Vector3 _lookAtTarget;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        _playerInput.Player.Look.performed += HandleLook;
        _playerInput.Player.Look.canceled += HandleLookCancel;
        _playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.Look.performed -= HandleLook;
        _playerInput.Player.Look.canceled -= HandleLookCancel;
        _playerInput.Player.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Quaternion initialRotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0f);
        _rawPosition = (_target.position + Vector3.up * _height) + (initialRotation * (-Vector3.forward * _distance));
        _camera.transform.position = _rawPosition;
    }

    private void HandleLook(InputAction.CallbackContext ctx)
    {
        _inputVector = ctx.ReadValue<Vector2>();
    }

    private void HandleLookCancel(InputAction.CallbackContext ctx)
    {
        _inputVector = Vector2.zero;
    }

    private void LateUpdate()
    {
        UpdateRotation();
        UpdatePosition();
        _camera.transform.LookAt(_lookAtTarget);
    }

    private void UpdateRotation()
    {
        _horizontalRotation += _inputVector.x * _horizontalRotationSpeed * Time.deltaTime;
        _verticalRotation -= _inputVector.y * _verticalRotationSpeed * Time.deltaTime;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _lowestVerticalLimit, _highestVerticalLimit);

        IsMoving = _inputVector != Vector2.zero;
    }

    private void UpdatePosition()
    {
        Quaternion targetRotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0f);
        Vector3 pivotPosition = _target.position + Vector3.up * _height;

        _rawPosition = pivotPosition + (targetRotation * (-Vector3.forward * _distance));

        ApplyCollision(pivotPosition, targetRotation);
    }

    private void ApplyCollision(Vector3 pivotPosition, Quaternion targetRotation)
    {
        Vector3 rawPositionDirection = _rawPosition - pivotPosition;
        float rawDistance = rawPositionDirection.magnitude;

        float finalDistance = rawPositionDirection.magnitude;

        if (Physics.SphereCast(pivotPosition, _sphereCastCollisionRadius, rawPositionDirection.normalized, out RaycastHit hit, rawDistance, _collisionLayerMask))
        {
            finalDistance = Mathf.Max(_sphereCastCollisionRadius, hit.distance);
            _camera.transform.position = pivotPosition + (rawPositionDirection.normalized * finalDistance);

            float distanceRatio = finalDistance / _distance;
            ApplyVisibility(distanceRatio);
            Vector3 dynamicOffset = _offset * distanceRatio;

            SetLookAtTarget(pivotPosition, targetRotation, dynamicOffset);
            return;
        }
        else
        {
            _camera.transform.position = pivotPosition + (rawPositionDirection.normalized * finalDistance);
            SetLookAtTarget(pivotPosition, targetRotation, _offset);

            _characterBody.SetActive(true);
        }
    }

    private void SetLookAtTarget(Vector3 pivotPosition, Quaternion targetRotation, Vector3 offset)
    {
        _lookAtTarget = pivotPosition + (targetRotation * offset);
    }

    private void ApplyVisibility(float distanceRatio)
    {
        if (_characterBody != null && distanceRatio < _visibilityThreshold)
        {
            _characterBody.SetActive(false);
        }
        else
        {
            _characterBody.SetActive(true);
        }
    }

    public Vector3 GetCameraFlatForward()
    {
        Vector3 forward = _camera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
}
