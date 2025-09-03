using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;
    [SerializeField] private Graphic target;
    private float _speed = 1f;

    private void Awake()
    {
        // �켱 Ŭ�����ε� ������ �� �ְ� �д�.
        gameStartBtn.onClick.AddListener(() =>
        {
            GameManager.Scene.LoadScene(Scenes.TEST);
        });
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            GameManager.Scene.LoadScene(Scenes.TEST);
        }

        float t = Mathf.PingPong(Time.time * _speed, 1f);
        float a = Mathf.Lerp(30f / 200f, 1f, t);

        var c = target.color;
        c.a = a;
        target.color = c;
    }
}
