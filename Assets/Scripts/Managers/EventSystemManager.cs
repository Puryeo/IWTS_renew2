using UnityEngine;

public class EventSystemSingleton : MonoBehaviour
{
    private static EventSystemSingleton instance;

    void Awake()
    {
        if (instance != null)
        {
            // �̹� �����ϴ� ��� ���� ������ EventSystem ����
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
