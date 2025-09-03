using UnityEngine;

// �ߺ� ����, DontDestroyOnLoad, ���� ���� ���� ���̽�
public abstract class MonoManager<T> : MonoBehaviour where T : MonoManager<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        // �̹� ����ִ� �ν��Ͻ��� ������ �ڽ��� ����(�ߺ� ����)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
        DontDestroyOnLoad(gameObject);
        OnManagerAwake();
    }
    
    // �ߺ� �� ����
    protected virtual void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    // �Ļ� Ŭ�������� �ʱ�ȭ �ڵ�
    protected virtual void OnManagerAwake()
    {

    }
}
