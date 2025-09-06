using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 입력에 따라 카메라 시점을 회전하고, 지정된 대상을 부드럽게 따라다니는 스크립트입니다.
/// 벽 충돌 방지 기능이 포함되어 있습니다.
/// This script rotates the camera view based on mouse input, smoothly follows a designated target, and includes wall collision avoidance.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("설정 (Settings)")]
    [Tooltip("카메라가 따라갈 대상입니다. (The target for the camera to follow.)")]
    [SerializeField] private Transform _target;
    [Tooltip("마우스 감도입니다. (Mouse sensitivity.)")]
    [SerializeField] private float _mouseSensitivity = 2.0f;
    [Tooltip("대상과의 기본 거리입니다. (Default distance from the target.)")]
    [SerializeField] private float _distanceFromTarget = 5.0f;

    [Header("부드러운 움직임 (Smoothing)")]
    [Tooltip("카메라 회전의 부드러운 정도입니다. (The smoothness of the camera rotation.)")]
    [SerializeField] private float _rotationSmoothTime = 0.12f;
    [Tooltip("카메라 위치 이동의 부드러운 정도입니다. (The smoothness of the camera position movement.)")]
    [SerializeField] private float _positionSmoothTime = 0.2f;

    [Header("카메라 각도 제한 (Angle Limits)")]
    [Tooltip("카메라의 최소/최대 상하 각도입니다. (Min/max vertical angle of the camera.)")]
    [SerializeField] private Vector2 _pitchMinMax = new Vector2(-40, 85);

    // 추가됨: 벽 충돌 방지를 위한 설정
    // ADDED: Settings for wall collision avoidance
    [Header("충돌 방지 (Collision Avoidance)")]
    [Tooltip("카메라 충돌을 감지할 레이어 마스크입니다. (Layer mask to detect camera collisions.)")]
    [SerializeField] private LayerMask _collisionMask;
    [Tooltip("충돌 시 카메라와 벽 사이의 최소 거리입니다. (Minimum distance between the camera and a wall upon collision.)")]
    [SerializeField] private float _collisionPadding = 0.35f;


    private float _yaw;   // 좌우 회전 (Y-axis)
    private float _pitch; // 상하 회전 (X-axis)

    // 부드러운 움직임을 위한 변수들
    // Variables for smoothing
    private Vector3 _currentRotation;
    private Vector3 _rotationSmoothVelocity;
    private Vector3 _currentPositionVelocity;

    void Start()
    {
        if (_target == null)
        {
            Debug.LogError("카메라의 Target이 설정되지 않았습니다! 플레이어 오브젝트를 연결해주세요. (Camera target is not set! Please assign the player object.)");
            return;
        }
        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        _yaw += lookInput.x * _mouseSensitivity;
        _pitch -= lookInput.y * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _pitchMinMax.x, _pitchMinMax.y);
    }

    void LateUpdate()
    {
        if (_target == null) return;

        // 1. 목표 회전값을 계산하고 부드럽게 변경합니다.
        // 1. Calculate the target rotation and smoothly change it.
        Vector3 targetRotation = new Vector3(_pitch, _yaw);
        _currentRotation = Vector3.SmoothDamp(_currentRotation, targetRotation, ref _rotationSmoothVelocity, _rotationSmoothTime);
        transform.eulerAngles = _currentRotation;

        // 2. 플레이어 위치에서 회전 방향으로 거리를 둔 위치를 카메라의 기본 목표 위치로 설정합니다.
        // 2. Set the camera's default target position at a distance from the player, based on the current rotation.
        Vector3 targetPosition = _target.position - transform.forward * _distanceFromTarget;

        // 추가됨: 벽 충돌 처리
        // ADDED: Wall collision handling
        targetPosition = HandleCollision(targetPosition);

        // 3. 수정됨: Lerp 대신 SmoothDamp를 사용하여 현재 위치에서 목표 위치로 부드럽게 이동합니다.
        // 3. MODIFIED: Smoothly move from the current position to the target position using SmoothDamp instead of Lerp.
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentPositionVelocity, _positionSmoothTime);
    }

    /// <summary>
    /// 카메라와 플레이어 사이에 장애물이 있는지 확인하고, 있다면 카메라 위치를 조정합니다.
    /// Checks for obstacles between the camera and the player, adjusting the camera position if necessary.
    /// </summary>
    private Vector3 HandleCollision(Vector3 targetPosition)
    {
        RaycastHit hit;
        // 플레이어의 위치에서 카메라의 목표 위치 방향으로 광선을 쏩니다.
        // Cast a ray from the player's position towards the camera's target position.
        if (Physics.Linecast(_target.position, targetPosition, out hit, _collisionMask))
        {
            // 광선이 어딘가에 부딪혔다면, 부딪힌 지점에서 약간의 여유(padding)를 둔 위치를 새로운 목표 위치로 합니다.
            // If the ray hits something, the new target position becomes the point of collision, plus a small padding.
            Vector3 direction = (hit.point - _target.position).normalized;
            return _target.position + direction * (hit.distance - _collisionPadding);
        }

        // 부딪힌 곳이 없다면 원래 목표 위치를 그대로 반환합니다.
        // If there's no collision, return the original target position.
        return targetPosition;
    }
}

