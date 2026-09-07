using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;

    public UnityEvent OnDeath;

    [Header("Efectos al morir")]
    public float deathShakeIntensity = 0.6f;
    public float deathShakeDuration = 0.2f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        
        // --- NUEVO: Camera Shake al morir ---
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeCamera(deathShakeIntensity, deathShakeDuration);
        }

        Destroy(gameObject);
    }
}
