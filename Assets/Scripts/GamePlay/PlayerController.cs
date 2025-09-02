using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // �̵� ���� ����
    [Header("Movement Settings")]
    // �ִ� �̵� �ӵ� (Inspector���� ���� ����)
    public float _moveSpeed = 5f;
    // �������� �ִ� �ӵ��� ������ �������� ���ӷ�(Ŭ���� �� ���� �ӵ� ����)
    public float _acceleration = 20f;
    // �Է� ���� �� ���ӷ�(Ŭ���� �� ������ ����)
    public float _deceleration = 25f;
    // ĳ���Ͱ� �̵� ������ �ٶ󺸴� ȸ�� �ӵ�(��/��)
    public float _rotationSpeed = 720f; // degrees per second

    // �Է� ����
    [Header("Input")]
    // PlayerControls ���� ���� 'Move' �׼��� ������ InputActionReference.
    // �� �ʵ忡 ���¿��� ������ Move �׼��� �巡���ؼ� �����ϼ��� (Vector2).
    public InputActionReference moveAction;

    // ���� ���� ����� �ʵ�
    Rigidbody _rb; // �� ��ũ��Ʈ�� ������ Rigidbody ������Ʈ
    Vector2 _moveInput; // �ֽ� �Է°� (x: ��/��, y: ��/��)
    Vector3 _currentPlanarVelocity; // X-Z ��鿡�� ���� ��ǥ �ӵ�(�߷� ������ Y ������Ʈ�� ���� ����)

    // Awake: ������Ʈ �ʱ�ȭ �� Rigidbody ���� ����
    void Awake()
    {
        // Rigidbody ������Ʈ ȹ��
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            // ������ ��Ÿ�ӿ� �߰��ϰ� ��� ���
            Debug.LogWarning("PlayerController requires a Rigidbody. Adding one at runtime.");
            _rb = gameObject.AddComponent<Rigidbody>();
        }

        // ��翡 ���� Rigidbody ���� ����
        // Y ��ġ ������ �����Ͽ� �߷��� �����ϵ��� ��. �ٸ� X/Z �� ȸ���� �����Ͽ� ��鸲 ����.
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // �ʱ� ��� �ӵ��� Rigidbody�� ���� �ӵ����� X,Z�� ����
        // (Y ������ �߷�/���� �� ���� �ý��ۿ� �ñ�)
        _currentPlanarVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
    }

    // OnEnable: InputAction �ݹ� ��� �� Ȱ��ȭ
    void OnEnable()
    {
        // moveAction�� ��ȿ�ϸ� performed�� canceled �̺�Ʈ�� ����
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
            moveAction.action.Enable();
        }
        else
        {
            // ���� ���� �� ���
            Debug.LogWarning("Move ActionReference is not assigned on PlayerController. Assign the PlayerControls 'Move' action in the Inspector.");
        }
    }

    // OnDisable: �ݹ� ���� �� ��Ȱ��ȭ
    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }
    }

    // �Է��� �߻����� �� ȣ��Ǵ� �ݹ�
    // performed �̺�Ʈ���� �Է� ���͸� �о� _moveInput�� ����
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    // �Է��� ��ҵǾ��� �� ȣ��Ǵ� �ݹ�
    // �Է��� �ʱ�ȭ�Ͽ� ���� �������� �ڿ������� ���߰� ��
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _moveInput = Vector2.zero;
    }

    // FixedUpdate: ��� ���� ó��(�ӵ� ���� �� ȸ��)�� ���⼭ ����
    void FixedUpdate()
    {
        // ��ǥ ��� �ӵ� ��� (�Է� x->X, �Է� y->Z)
        Vector3 desiredPlanar = new Vector3(_moveInput.x, 0f, _moveInput.y) * _moveSpeed;

        // ����� ��ǥ �ӵ��� ũ�� �񱳷� ����/���� ���� ����
        float currentMag = _currentPlanarVelocity.magnitude;
        float desiredMag = desiredPlanar.magnitude;

        // �� �����ӿ��� ����� �ִ� �ӵ� ��ȭ�� ���
        float maxDelta = (desiredMag > currentMag) ? _acceleration * Time.fixedDeltaTime : _deceleration * Time.fixedDeltaTime;

        // �ӵ��� �ε巴�� ��ǥ �ӵ��� �̵���Ŵ (���۽����� ��ȭ ����)
        _currentPlanarVelocity = Vector3.MoveTowards(_currentPlanarVelocity, desiredPlanar, maxDelta);

        // Rigidbody�� velocity�� ���� �����ϵ� Y ������ ���� ���� �����Ͽ� �߷�/���ϸ� ���
        _rb.linearVelocity = new Vector3(_currentPlanarVelocity.x, _rb.linearVelocity.y, _currentPlanarVelocity.z);

        // �̵� ���� ���� ĳ���Ͱ� �̵� ������ �ٶ󺸵��� ȸ�� ����
        if (_currentPlanarVelocity.sqrMagnitude > 0.001f)
        {
            Vector3 lookDir = _currentPlanarVelocity.normalized;
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            // �ε巯�� ȸ�� (�ִ� ȸ������ _rotationSpeed * deltaTime)
            Quaternion newRot = Quaternion.RotateTowards(_rb.rotation, targetRot, _rotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(newRot);
        }
    }

    // �����Ϳ��� �� �Է� �� �ּҰ� ����
    void OnValidate()
    {
        _moveSpeed = Mathf.Max(0f, _moveSpeed);
        _acceleration = Mathf.Max(0f, _acceleration);
        _deceleration = Mathf.Max(0f, _deceleration);
        _rotationSpeed = Mathf.Max(0f, _rotationSpeed);
    }
}
