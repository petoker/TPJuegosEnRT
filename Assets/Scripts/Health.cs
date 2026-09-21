using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;
    private bool isDead;

    [Header("Comportamiento")]
    [Tooltip("Si es false, el objeto no se destruye al morir (permite dejar el cuerpo/cadaver)")]
    public bool destroyOnDeath = true;

    [Header("Eventos")]
    public UnityEvent OnDamaged;
    public UnityEvent OnDeath;

    [Header("Efectos al morir")]
    public float deathShakeIntensity = 0.6f;
    public float deathShakeDuration = 0.2f;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();

        // --- Camera Shake al morir ---
        if (CameraController.Instance != null)
        {
            CameraController.Instance.ShakeCamera(deathShakeIntensity, deathShakeDuration);
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}