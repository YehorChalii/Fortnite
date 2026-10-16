using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CharacterControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CameraControl _cameraControl;

    [Header("Visuals")]
    [SerializeField] private GameObject _characterPickAxe;
    [SerializeField] private GameObject _characterBlueprint;

    [Space]
    [Header("Speed")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _rotationSpeed;

    [Space]
    [Header("Jump")]
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _jumpCooldown;
    [SerializeField] private float _gravityForce;

    [Space]
    [SerializeField, Range(0f, 180f)] private float _maxAngleDifferenceWithCamera;

    private bool _canJump = true;
    private bool _inAir;
    private float _afterJumpTimer;
    private float _afterJumpThreshold = 0.5f;
    private bool _hasLanded;
    private float _jumpCooldownTimer;

    private PlayerInput _playerInput;
    private Vector3 _jumpForce;
    private Vector2 _inputVector;
    private Vector3 _velocity;

    private bool _isMoving;
    private bool _isRotating;

    private Vector3 _slopeVelocity;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        _playerInput.Player.Move.performed += HandleMove;
        _playerInput.Player.Move.canceled += HandleMoveCancel;
        _playerInput.Player.Jump.performed += HandleJump;
        _playerInput.Player.Enable();

        BuildingManager.OnBuildingModeToggle += HandleBuildingModeToggle;
    }

    private void OnDisable()
    {
        _playerInput.Player.Move.performed -= HandleMove;
        _playerInput.Player.Move.canceled -= HandleMoveCancel;
        _playerInput.Player.Jump.performed -= HandleJump;
        _playerInput.Player.Disable();

        BuildingManager.OnBuildingModeToggle -= HandleBuildingModeToggle;
    }

    private void HandleBuildingModeToggle(bool buildingMode)
    {
        _characterPickAxe.SetActive(!buildingMode);
        _characterBlueprint.SetActive(buildingMode);
    }

    private void Start()
    {
        _jumpForce = -Physics.gravity.normalized * Mathf.Sqrt(2f * Physics.gravity.magnitude * _jumpHeight);

        _characterPickAxe.SetActive(true);
        _characterBlueprint.SetActive(false);
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        _inputVector = ctx.ReadValue<Vector2>();
    }

    private void HandleMoveCancel(InputAction.CallbackContext ctx)
    {
        _inputVector = Vector2.zero;
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (_characterController.isGrounded && _canJump)
        {
            _velocity += _jumpForce;
            _canJump = false;
            _inAir = true;
        }
    }

    private void Update()
    {
        ApplyGravity();
        ApplyMovement();
        CheckForRotation();
        CheckLand();
        _characterController.Move((_velocity + _slopeVelocity) * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && !_inAir)
        {
            _velocity.y = -2f;
        }
        else
        {
            _velocity.y += -_gravityForce * Time.deltaTime;           
        }
    }

    private void ApplyMovement()
    {
        _isMoving = _inputVector != Vector2.zero;
        _slopeVelocity = Vector3.zero;

        if (_isMoving)
        {
            Vector2 directionalSpeedInputVector = GetDirectionalSpeedInputVector(_inputVector.normalized);
            Vector3 cameraRelativeInputDirection = GetCameraRelativeInputDirection(directionalSpeedInputVector);

            Vector3 floorNormal = GetFloorNormal();
            float slopeAngle = Vector3.Angle(floorNormal, Vector3.up);

            Vector3 finalMovementDirection = cameraRelativeInputDirection;

            if (slopeAngle > 0 && slopeAngle <= _characterController.slopeLimit &&
                !_inAir && _characterController.isGrounded)
            {
                finalMovementDirection = Vector3.ProjectOnPlane(cameraRelativeInputDirection, floorNormal).normalized;
                _slopeVelocity = new Vector3(0f, finalMovementDirection.y * _movementSpeed, 0f);
            }

            _velocity.x = finalMovementDirection.x * _movementSpeed;
            _velocity.z = finalMovementDirection.z * _movementSpeed;
        }
        else
        {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }
    }

    private Vector3 GetFloorNormal()
    {
        Vector3 rayOrigin = _characterController.transform.position;

        float offset = 0.5f;
        float rayLength = (_characterController.height / 2f) + offset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayLength))
        {
            return hit.normal;
        }

        return Vector3.up;
    }

    private void CheckForRotation()
    {
        Vector3 cameraForward = _cameraControl.GetCameraFlatForward();
        float angleDifference = Vector3.Angle(_characterController.transform.forward, cameraForward);

        if (_isMoving)
        {
            ApplyRotation(cameraForward);
            return;
        }

        if (!_isRotating && angleDifference >= _maxAngleDifferenceWithCamera)
        {
            _isRotating = true;
        }

        if (_isRotating)
        {
            ApplyRotation(cameraForward);
        }
    }

    private void ApplyRotation(Vector3 cameraForward)
    {
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

        float angleDifference = Vector3.Angle(transform.forward, cameraForward);

        if (angleDifference <= 0.5f && !_cameraControl.IsMoving)
        {
            _isRotating = false;
        }
    }


    private void CheckLand()
    {
        if (_inAir)
        {
            _afterJumpTimer += Time.deltaTime;
            if (_afterJumpTimer <= _afterJumpThreshold) return;
        }

        if (_inAir && _characterController.isGrounded)
        {
            _afterJumpTimer = 0;

            _inAir = false;
            _hasLanded = true;
        }

        if (_hasLanded)
        {
            _jumpCooldownTimer += Time.deltaTime;
            if (_jumpCooldownTimer >= _jumpCooldown)
            {
                _canJump = true;
                _hasLanded = false;
                _jumpCooldownTimer = 0f;
            }
        }
    }

    private Vector3 GetDirectionalSpeedInputVector(Vector2 inputVector)
    {
        if(inputVector.y < 0f)
        {
            inputVector.y *= 0.5f;
        }
        if(Mathf.Abs(inputVector.x) > 0f)
        {
            inputVector.x *= 0.75f;
        }

        return inputVector;
    }

    private Vector3 GetCameraRelativeInputDirection(Vector2 inputVector)
    {
        return Quaternion.LookRotation(_cameraControl.GetCameraFlatForward()) * new Vector3(inputVector.x, 0f, inputVector.y);
    }
}