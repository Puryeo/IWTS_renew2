using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 플레이어의 이동과 물리만 담당하는 스크립트입니다. 카메라 제어 로직이 완전히 분리되었습니다.
/// This script is responsible only for player movement and physics. Camera control logic is completely separated.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region 이벤트 (Events)
    public event Action<string> OnGoalReached;
    #endregion

    #region 인스펙터 변수 (Inspector Variables)

    [Header("MOVEMENT SETTINGS")]
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _acceleration = 80f;
    [SerializeField] private float _deceleration = 120f;

    [Header("JUMP SETTINGS")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _jumpGravityMultiplier = 2.0f;
    [SerializeField] private float _fallMultiplier = 3.0f;
    [SerializeField][Range(0f, 1f)] private float _airControlMultiplier = 0.7f;

    [Header("RESPONSIVENESS BUFFERS")]
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.15f;

    [Header("GROUND DETECTION")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("PLAYER ROTATION SETTINGS")]
    [Tooltip("캐릭터가 이동 방향으로 회전하는 속도입니다. (The speed at which the character turns to face the movement direction.)")]
    [SerializeField] private float _rotationSpeed = 15f;

    #endregion

    #region 내부 상태 및 출력 변수 (Internal State & Output)

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private bool _hasCollidedWithGoal = false;

    private Transform _mainCameraTransform; // 카메라 방향을 참고하기 위한 변수

    [Header("DEBUG OUTPUT")]
    public Vector2 RelativeInputDirection;

    #endregion

    #region 유니티 라이프사이클 (Unity Lifecycle)
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 메인 카메라의 Transform을 찾아서 저장해 둡니다.
        _mainCameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleState();
        HandleTimers();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation(); // 이동 방향으로 회전하는 로직을 추가
        HandleJump();
        HandleGravity();
    }
    #endregion

    #region 입력 처리 (Input Handling)
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            _jumpBufferCounter = _jumpBufferTime;
    }

    // ✨ OnLook 메서드는 PlayerController에서 완전히 제거합니다. 카메라는 CameraController가 제어합니다.
    // ✨ The OnLook method is completely removed from PlayerController. The camera is controlled by CameraController.
    #endregion

    #region 상태 판정 및 타이머 (State & Timers)
    private void HandleState()
    {
        Transform groundCheckOrigin = _groundCheckPoint != null ? _groundCheckPoint : transform;
        _isGrounded = Physics.SphereCast(groundCheckOrigin.position, _groundCheckRadius, Vector3.down, out _, _groundCheckDistance, _groundLayer);
    }

    private void HandleTimers()
    {
        _coyoteTimeCounter = _isGrounded ? _coyoteTime : _coyoteTimeCounter - Time.deltaTime;
        _jumpBufferCounter = _jumpBufferCounter > 0 ? _jumpBufferCounter - Time.deltaTime : 0;
    }
    #endregion

    #region 물리 처리 (Physics Handling)

    private Vector3 _moveDirection; // 이동 방향을 다른 메서드에서 사용하기 위해 멤버 변수로 변경

    private void HandleMovement()
    {
        // 이동 방향을 카메라의 시점을 기준으로 계산합니다.
        Vector3 camForward = _mainCameraTransform.forward;
        Vector3 camRight = _mainCameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;

        _moveDirection = (camForward.normalized * _moveInput.y + camRight.normalized * _moveInput.x);
        if (_moveDirection.sqrMagnitude > 1f)
            _moveDirection.Normalize();

        RelativeInputDirection = new Vector2(_moveDirection.x, _moveDirection.z);

        float controlMultiplier = _isGrounded ? 1f : _airControlMultiplier;
        Vector3 targetVelocity = _moveDirection * _moveSpeed * controlMultiplier;

        float acceleration = _moveDirection.sqrMagnitude > 0.01f ? _acceleration : _deceleration;
        Vector3 currentPlanarVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        Vector3 newPlanarVelocity = Vector3.MoveTowards(currentPlanarVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector3(newPlanarVelocity.x, _rb.linearVelocity.y, newPlanarVelocity.z);
    }

    private void HandleRotation()
    {
        // 이동 입력이 있을 때만 이동 방향으로 캐릭터를 부드럽게 회전시킵니다.
        if (_moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleJump()
    {
        if (_coyoteTimeCounter > 0f && _jumpBufferCounter > 0f)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);

            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }
    }

    private void HandleGravity()
    {
        if (!_isGrounded)
        {
            float verticalVelocity = _rb.linearVelocity.y;

            if (verticalVelocity < 0)
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1f) * Time.fixedDeltaTime;
            else if (verticalVelocity > 0)
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_jumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }
    #endregion

    #region 충돌 처리 (Collision Handling)
    private void OnTriggerEnter(Collider other)
    {
        if (_hasCollidedWithGoal) return;

        if (other.CompareTag("Goal") || other.CompareTag("Hidden"))
        {
            _hasCollidedWithGoal = true;
            Debug.Log($"골 지점 도달: {other.tag} (Goal Reached: {other.tag})");
            OnGoalReached?.Invoke(other.tag);
        }
    }
    #endregion
}

