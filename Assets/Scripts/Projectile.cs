using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 2f;
    
    [HideInInspector] public GameObject shooter; // Referencia directa a quien disparó

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        HandleHit(hitInfo.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        // Evitar que la bala le haga daño a quien la disparó
        if (shooter != null && hitObject.transform.IsChildOf(shooter.transform))
        {
            return;
        }

        Debug.Log("La bala chocó contra: " + hitObject.name);

        Health health = hitObject.GetComponentInParent<Health>();
        if (health != null)
        {
            Debug.Log("¡Aplicando daño a " + health.gameObject.name + "!");
            health.TakeDamage(damage);
        }

        // Destruir la bala al chocar
        Destroy(gameObject);
    }
}
