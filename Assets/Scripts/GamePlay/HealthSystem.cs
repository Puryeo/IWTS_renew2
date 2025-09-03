using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public bool IsDead => currentHealth <= 0f;
    public UnityEngine.Events.UnityEvent onDamaged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"ü��: {currentHealth}");

        // �̺�Ʈ ȣ�� ����
        onDamaged?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("���! �������� ó������ ���ư��ϴ�.");
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); // ���� �÷����� �� ó�� �ҷ�����
    }

    public void RestoreFull()
    {
        currentHealth = maxHealth;
    }
}
