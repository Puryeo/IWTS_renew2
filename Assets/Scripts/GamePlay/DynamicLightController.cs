using UnityEngine;

/// <summary>
/// Directional Light�� �������� ȸ����Ű��, ���� �߾Ӱ�(fixedElevation)�� ��������
/// ������ ������ŭ ���Ʒ��� �ֱ������� ���ϰ� �մϴ�.
/// - ���� ȸ���� ���� ���(�ʴ� ����)���� ����Ͽ� �Ͻ����� ��� �� �������Դϴ�.
/// - ���� �����ķ� ��ȭ�ϸ�, �ν����Ϳ��� �߾Ӱ�(fixedElevation), ����(elevationAmplitudeDeg), �ӵ�(elevationSpeed)�� ������ �� �ֽ��ϴ�.
/// </summary>
[RequireComponent(typeof(Light))]
public class SimpleSunController : MonoBehaviour
{
    [Header("�¾� ����")]
    [Tooltip("�¾��� �������� ȸ���ϴ� �ӵ��Դϴ�. (����: ��/��)")]
    public float rotationSpeed = 10f;

    [Tooltip("�¾��� ������ �߾� ����(��)�Դϴ�. �� ���� �߽����� ��/�Ʒ��� �����մϴ�.")]
    [Range(0f, 90f)]
    public float fixedElevation = 45f;

    [Tooltip("fixedElevation�� �������� ��/�Ʒ��� ��鸮�� ����(��). ���� ���� fixedElevation �� amplitude�� �˴ϴ�.")]
    public float elevationAmplitudeDeg = 15f;

    [Tooltip("�� ���� �ӵ�(���ļ�, ����: �ֱ�/��). ���� Ŭ���� �� ��ȭ�� �������ϴ�.")]
    public float elevationSpeed = 0.2f;

    // ���� ���� ����(��) - Time.time ��� deltaTime �������� ȸ�� ���¸� ����
    private float _azimuthAngleDeg;
    // ���� �ð� ����(�� ���� ����)
    private float _time;

    void Update()
    {
        // �ð� ����
        _time += Time.deltaTime;

        // ��������(����) �� ����: deltaTime ������� ������
        _azimuthAngleDeg += rotationSpeed * Time.deltaTime;

        // �������� ���� 0-360 ������ �����Ͽ� ��ġ�� ��� Ŀ���� �ʵ��� ��
        _azimuthAngleDeg = Mathf.Repeat(_azimuthAngleDeg, 360f);

        // ���� ȸ�� ���
        Quaternion horizontalRotation = Quaternion.Euler(0f, _azimuthAngleDeg, 0f);

        // ��(�������̼�)�� �����ķ� ��ȭ��Ű��
        // elevationSpeed�� '�ֱ�/��'�� �ؼ��Ͽ� ���������� 2�� ���� ���� �Է����� ���
        float elevationOffset = Mathf.Sin(_time * elevationSpeed * Mathf.PI * 2f) * elevationAmplitudeDeg;
        float currentElevation = fixedElevation + elevationOffset;

        // ���� ȸ��(��) ���
        Quaternion verticalRotation = Quaternion.Euler(currentElevation, 0f, 0f);

        // ���յ� ȸ�� ���� (���� * ����)
        transform.rotation = horizontalRotation * verticalRotation;
    }

    private void OnValidate()
    {
        // ���� ������ ����
        rotationSpeed = rotationSpeed;
        fixedElevation = Mathf.Clamp(fixedElevation, 0f, 90f);
        elevationAmplitudeDeg = Mathf.Max(0f, elevationAmplitudeDeg);
        elevationSpeed = Mathf.Max(0f, elevationSpeed);

        // ������ �ʹ� Ŀ�� ���� ������ ����ġ�� ū ���� ���� �ʵ��� ���� ����(�ڵ� ������ ���� ����)
    }
}