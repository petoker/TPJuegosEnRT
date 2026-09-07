using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public Transform target; // El jugador
    public float moveSpeed = 3f;
    public float stoppingDistance = 5f; // Distancia para empezar a disparar
    
    private Rigidbody2D rb;
    private Weapon weapon;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        weapon = GetComponentInChildren<Weapon>(); // Asume que el enemigo tiene un arma hija

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        // Mirar hacia el jugador
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Calcular distancia
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > stoppingDistance)
        {
            // Moverse hacia el jugador
            rb.MovePosition(rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Disparar si está lo suficientemente cerca y tiene un arma
            if (weapon != null)
            {
                weapon.Shoot();
            }
        }
    }
}
