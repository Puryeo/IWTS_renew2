using UnityEngine;

public class LightSystem : MonoBehaviour
{
    public RenderTexture[] rTexture;   // Ȯ���� RenderTexture
    private Texture2D tex2D;
    private bool isExposedToLight = false;
    private int textureSize;
    public float lightThreshold = 0.48f;

    void Start()
    {
        // �ؽ��� �ϳ� �����ͼ� ������ ����
        textureSize = rTexture[0].width;

        tex2D = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
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

    void Update()
    {
        isExposedToLight = CheckWhitePixel();

        if (isExposedToLight)
        {
            Debug.Log("���� ����Ǿ����ϴ�!");
        }
    }
}
