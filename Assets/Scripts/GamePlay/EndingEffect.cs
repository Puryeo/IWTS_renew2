using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingEffect : MonoBehaviour
{
    [Header("Refs")]
    public GameObject player;
    public Transform coffinTransform;
    public Camera mainCamera;
    public RawImage irisImage;         // Iris ���̴��� �� ��Ƽ������ �Ҵ��� RawImage

    [Header("Timings")]
    public float cameraMoveDuration = 3f;  // ī�޶� Ŭ����� �ð�
    public float irisSpeed = 1f;           // ���̸��� ���� �ӵ� ���(1�̸� �뷫 1��)

    [Header("Camera Offset")]
    public Vector3 endOffset = new Vector3(0f, 1f, -2f); // �� ���� ī�޶� ������

    public bool isPlaying { get; private set; }
    public bool isFinished { get; private set; }

    // ���̴� ������Ƽ ����: _Scale(float: ������), _Center(float2: 0..1)
    private Material irisMaterial;

    void Awake()
    {
        if (irisImage != null)
        {
            irisMaterial = irisImage.material;   // �ν��Ͻ�ȭ �� �� (��û �ݿ�)
            irisImage.enabled = false;           // ���� �� ����
            irisImage.raycastTarget = false;
            // RawImage ������ ���/����1 ����(��Ƽ������ ���� ����)
            if (irisImage.color.a < 1f) irisImage.color = new Color(1, 1, 1, 1);

            SetIrisScale(1f); // ������ ȭ�� ���� ���� ����
        }
    }

    public void PlayEnding()
    {
        if (isPlaying || mainCamera == null || coffinTransform == null || irisMaterial == null || irisImage == null)
            return;

        isPlaying = true;
        StartCoroutine(EndingCoroutine());
    }

    private IEnumerator EndingCoroutine()
    {
        isFinished = false;

        // 1) ī�޶� �� ������ Ŭ�����
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = coffinTransform.position + endOffset;
        Quaternion endRot = Quaternion.LookRotation(coffinTransform.position - endPos, Vector3.up);

        float t = 0f;
        float dur = Mathf.Max(0.01f, cameraMoveDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = SmoothStep01(t);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, e);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, e);
            yield return null;
        }
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        // 2) ���̸��� �ƿ�: �߽��� "��" ��ġ�� �����ϰ� �ݱ� ����
        irisImage.enabled = true;

        // �� �߽��� ScreenPoint��UV(0..1)�� ��ȯ(ĵ���� ���� �����ϰ� ��Ȯ)
        Vector3 sp = RectTransformUtility.WorldToScreenPoint(mainCamera, coffinTransform.position);
        float u = sp.x / Screen.width;
        float v = sp.y / Screen.height;
        irisMaterial.SetVector("_Center", new Vector4(u, v, 0f, 0f));

        // �� ���̵� ȭ�� �����ڸ� ���� ���� ���� ���� �������� �˳���
        float startScale = 1.2f; // 1�� �����ϸ� 1.3~1.4������ ����
        float endScale = 0f;
        SetIrisScale(startScale);

        t = 0f;
        float speed = Mathf.Max(0.01f, irisSpeed);
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float e = SmoothStep01(t);
            SetIrisScale(Mathf.Lerp(startScale, endScale, e));
            yield return null;
        }
        SetIrisScale(0f); // ���� ����(��)

        isFinished = true;
        isPlaying = false;
    }

    private void SetIrisScale(float v)
    {
        if (irisMaterial != null) irisMaterial.SetFloat("_Scale", v);
    }

    private static float SmoothStep01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
