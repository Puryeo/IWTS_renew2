using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ���콺 �Է¿� ���� ī�޶� ������ ȸ���ϰ�, ������ ����� �ε巴�� ����ٴϴ� ��ũ��Ʈ�Դϴ�.
/// �� �浹 ���� ����� ���ԵǾ� �ֽ��ϴ�.
/// This script rotates the camera view based on mouse input, smoothly follows a designated target, and includes wall collision avoidance.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("���� (Settings)")]
    [Tooltip("ī�޶� ���� ����Դϴ�. (The target for the camera to follow.)")]
    [SerializeField] private Transform _target;
    [Tooltip("���콺 �����Դϴ�. (Mouse sensitivity.)")]
    [SerializeField] private float _mouseSensitivity = 2.0f;
    [Tooltip("������ �⺻ �Ÿ��Դϴ�. (Default distance from the target.)")]
    [SerializeField] private float _distanceFromTarget = 5.0f;

    [Header("�ε巯�� ������ (Smoothing)")]
    [Tooltip("ī�޶� ȸ���� �ε巯�� �����Դϴ�. (The smoothness of the camera rotation.)")]
    [SerializeField] private float _rotationSmoothTime = 0.12f;
    [Tooltip("ī�޶� ��ġ �̵��� �ε巯�� �����Դϴ�. (The smoothness of the camera position movement.)")]
    [SerializeField] private float _positionSmoothTime = 0.2f;

    [Header("ī�޶� ���� ���� (Angle Limits)")]
    [Tooltip("ī�޶��� �ּ�/�ִ� ���� �����Դϴ�. (Min/max vertical angle of the camera.)")]
    [SerializeField] private Vector2 _pitchMinMax = new Vector2(-40, 85);

    // �߰���: �� �浹 ������ ���� ����
    // ADDED: Settings for wall collision avoidance
    [Header("�浹 ���� (Collision Avoidance)")]
    [Tooltip("ī�޶� �浹�� ������ ���̾� ����ũ�Դϴ�. (Layer mask to detect camera collisions.)")]
    [SerializeField] private LayerMask _collisionMask;
    [Tooltip("�浹 �� ī�޶�� �� ������ �ּ� �Ÿ��Դϴ�. (Minimum distance between the camera and a wall upon collision.)")]
    [SerializeField] private float _collisionPadding = 0.35f;


    private float _yaw;   // �¿� ȸ�� (Y-axis)
    private float _pitch; // ���� ȸ�� (X-axis)

    // �ε巯�� �������� ���� ������
    // Variables for smoothing
    private Vector3 _currentRotation;
    private Vector3 _rotationSmoothVelocity;
    private Vector3 _currentPositionVelocity;

    void Start()
    {
        if (_target == null)
        {
            Debug.LogError("ī�޶��� Target�� �������� �ʾҽ��ϴ�! �÷��̾� ������Ʈ�� �������ּ���. (Camera target is not set! Please assign the player object.)");
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

        // 1. ��ǥ ȸ������ ����ϰ� �ε巴�� �����մϴ�.
        // 1. Calculate the target rotation and smoothly change it.
        Vector3 targetRotation = new Vector3(_pitch, _yaw);
        _currentRotation = Vector3.SmoothDamp(_currentRotation, targetRotation, ref _rotationSmoothVelocity, _rotationSmoothTime);
        transform.eulerAngles = _currentRotation;

        // 2. �÷��̾� ��ġ���� ȸ�� �������� �Ÿ��� �� ��ġ�� ī�޶��� �⺻ ��ǥ ��ġ�� �����մϴ�.
        // 2. Set the camera's default target position at a distance from the player, based on the current rotation.
        Vector3 targetPosition = _target.position - transform.forward * _distanceFromTarget;

        // �߰���: �� �浹 ó��
        // ADDED: Wall collision handling
        targetPosition = HandleCollision(targetPosition);

        // 3. ������: Lerp ��� SmoothDamp�� ����Ͽ� ���� ��ġ���� ��ǥ ��ġ�� �ε巴�� �̵��մϴ�.
        // 3. MODIFIED: Smoothly move from the current position to the target position using SmoothDamp instead of Lerp.
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentPositionVelocity, _positionSmoothTime);
    }

    /// <summary>
    /// ī�޶�� �÷��̾� ���̿� ��ֹ��� �ִ��� Ȯ���ϰ�, �ִٸ� ī�޶� ��ġ�� �����մϴ�.
    /// Checks for obstacles between the camera and the player, adjusting the camera position if necessary.
    /// </summary>
    private Vector3 HandleCollision(Vector3 targetPosition)
    {
        RaycastHit hit;
        // �÷��̾��� ��ġ���� ī�޶��� ��ǥ ��ġ �������� ������ ���ϴ�.
        // Cast a ray from the player's position towards the camera's target position.
        if (Physics.Linecast(_target.position, targetPosition, out hit, _collisionMask))
        {
            // ������ ��򰡿� �ε����ٸ�, �ε��� �������� �ణ�� ����(padding)�� �� ��ġ�� ���ο� ��ǥ ��ġ�� �մϴ�.
            // If the ray hits something, the new target position becomes the point of collision, plus a small padding.
            Vector3 direction = (hit.point - _target.position).normalized;
            return _target.position + direction * (hit.distance - _collisionPadding);
        }

        // �ε��� ���� ���ٸ� ���� ��ǥ ��ġ�� �״�� ��ȯ�մϴ�.
        // If there's no collision, return the original target position.
        return targetPosition;
    }
}

