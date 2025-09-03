using UnityEngine;

public class LightSystem : MonoBehaviour
{
    public RenderTexture[] rTexture;   // Ȯ���� RenderTexture
    private Texture2D tex2D;
    private bool isExposedToLight = false;
    private int textureSize;
    public float lightThreshold = 0.48f;

    // ü�� �ý���
    public HealthSystem healthSystem;
    public float damagePerSecond = 10f;

    void Start()
    {
        // �ؽ��� �ϳ� �����ͼ� ������ ����
        textureSize = rTexture[0].width;

        tex2D = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        healthSystem = GetComponent<HealthSystem>();
    }

    // RenderTexture���� �� ���̶� ���� ��� �ȼ��� �߰ߵǸ� true ��ȯ
    private bool CheckWhitePixel()
    {
        foreach (RenderTexture cTexture in rTexture)
        {
            RenderTexture.active = cTexture;
            tex2D.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            tex2D.Apply();
            RenderTexture.active = null;

            Color[] pixels = tex2D.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].r >= lightThreshold && pixels[i].g >= lightThreshold && pixels[i].b >= lightThreshold) // ���� ���
                {
                    return true; // �ϳ��� �߰� �� ��� true ��ȯ
                }
            }
        }

        return false;
    }


    private bool wasExposedToLight = false;  // ���� ������ ���� ����

    public GameObject headParticle;


    // �޺� �� �� �����ٰ� �� �ð� �� �ڵ����� ����
    public float particleShowSeconds = 2f;

    void Update()
    {
        // ���� �޺� ���� ����
        isExposedToLight = CheckWhitePixel();

        // ���� ������(���� �߿���)
        if (isExposedToLight && healthSystem != null)
            healthSystem.ApplyDamage(damagePerSecond * Time.deltaTime);

        // ���� ���� ����
        if (isExposedToLight && !wasExposedToLight)
        {
            // �޺��� ó�� ����
            Debug.Log("���� ���� ����!");
            SoundManager.Instance.StartLoopSFX(SoundId.longSizzle);

            if (headParticle != null)
            {
                headParticle.SetActive(true);
                CancelInvoke(nameof(DisableHeadParticle));
                Invoke(nameof(DisableHeadParticle), particleShowSeconds);
            }
        }
        else if (!isExposedToLight && wasExposedToLight)
        {
            // �׸��ڿ� ó�� ����
            Debug.Log("�� ���� ����!");
            SoundManager.Instance.StopLoopSFX();

            if (headParticle != null)
            {
                CancelInvoke(nameof(DisableHeadParticle));
                headParticle.SetActive(false);
            }
        }

        // ���� ����
        wasExposedToLight = isExposedToLight;
    }

    void DisableHeadParticle()
    {
        if (headParticle != null)
            headParticle.SetActive(false);
    }


}
