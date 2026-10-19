using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrulla (Ida y Vuelta)")]
    [Tooltip("Puedes arrastrar 1 o varios GameObjects vacíos como puntos de ruta")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;

    [Header("Cono de Visión (Linterna)")]
    [Tooltip("Objeto hijo que tiene el Sprite amarillo y un Collider2D con 'Is Trigger' activado")]
    public GameObject visionCone;
    public Transform target; // El jugador

    [Header("Animación")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("Efectos de Daño y Muerte")]
    [Tooltip("Sprite del cuerpo caído cuando muere")]
    public Sprite deadSprite;
    [Tooltip("Sprites de la animación al recibir impacto/daño")]
    public Sprite[] hitSprites;
    [Tooltip("Duración de la animación de impacto en segundos")]
    public float hitDuration = 0.2f;

    [Header("Ajustes de Rotación")]
    [Tooltip("Offset angular en grados si el sprite original mira hacia abajo (por defecto 90)")]
    public float rotationOffset = 90f;

    private Rigidbody2D rb;
    private Weapon weapon;
    private Health health;
    private Vector2 moveVelocity;
    private bool isDead;
    private bool isHitPlaying;

    // Variables simples para la patrulla y la visión
    private Vector2 startPosition;
    private int currentWaypointIndex = 0;
    private int patrolDirection = 1; // 1 = ida, -1 = vuelta
    private bool goingBackToStart = false; // Se usa cuando solo hay 1 punto asignado
    private bool playerInSight = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        weapon = GetComponentInChildren<Weapon>();
        health = GetComponent<Health>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Auto-detectar el cono de visión si es un hijo con Collider2D en modo Trigger
        if (visionCone == null)
        {
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            {
                if (col.gameObject != gameObject && col.isTrigger)
                {
                    visionCone = col.gameObject;
                    break;
                }
            }
        }

        if (health != null)
        {
            health.OnDamaged.AddListener(OnDamaged);
            health.OnDeath.AddListener(OnDeath);
        }
    }

    void Start()
    {
        // Guardamos la posición inicial por si solo le pones 1 GameObject para ir y volver
        startPosition = transform.position;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 1. Si el jugador está dentro del cono de visión -> Se frena, lo apunta y dispara
        if (playerInSight && target != null)
        {
            moveVelocity = Vector2.zero;
            UpdateAnimation(false);

            // Mirar hacia el jugador
            Vector2 dirToPlayer = (Vector2)target.position - rb.position;
            RotateTowards(dirToPlayer);

            // Disparar
            if (weapon != null && !isHitPlaying)
            {
                weapon.Shoot();
            }
        }
        // 2. Si NO ve al jugador -> Patrulla de ida y vuelta entre los puntos
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        // Si no tiene puntos asignados, se queda quieto
        if (waypoints == null || waypoints.Length == 0 || waypoints[0] == null)
        {
            moveVelocity = Vector2.zero;
            UpdateAnimation(false);
            return;
        }

        // Elegir hacia qué punto ir:
        // - Si solo pusiste 1 GameObject, va desde su posición inicial hasta ese objeto y vuelve.
        // - Si pusiste 2 o más GameObjects, va recorriéndolos en orden de ida y vuelta.
        Vector2 targetPoint;
        if (waypoints.Length == 1)
        {
            targetPoint = goingBackToStart ? startPosition : (Vector2)waypoints[0].position;
        }
        else
        {
            targetPoint = waypoints[currentWaypointIndex].position;
        }

        Vector2 toPoint = targetPoint - rb.position;
        float distance = toPoint.magnitude;

        // Si ya llegó al punto (distancia muy corta), cambia al siguiente punto
        if (distance < 0.15f)
        {
            if (waypoints.Length == 1)
            {
                goingBackToStart = !goingBackToStart;
            }
            else
            {
                currentWaypointIndex += patrolDirection;

                // Si llegó al último punto o volvió al primero, invierte la dirección (ida y vuelta)
                if (currentWaypointIndex >= waypoints.Length - 1)
                {
                    currentWaypointIndex = waypoints.Length - 1;
                    patrolDirection = -1;
                }
                else if (currentWaypointIndex <= 0)
                {
                    currentWaypointIndex = 0;
                    patrolDirection = 1;
                }
            }
        }
        else
        {
            // Moverse hacia el punto actual
            Vector2 moveDir = toPoint.normalized;
            moveVelocity = moveDir * moveSpeed;
            RotateTowards(moveDir);
            UpdateAnimation(true);
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        if (!isHitPlaying && direction.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Se activa automáticamente cuando el jugador entra al Collider2D (Is Trigger) del Cono de Visión
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSight = true;
            target = other.transform;
        }
    }

    // Se activa cuando el jugador sale del Cono de Visión
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSight = false;
        }
    }

    private void UpdateAnimation(bool isMoving)
    {
        if (isHitPlaying || isDead) return;

        if (animator != null && animator.enabled)
        {
            animator.SetBool("isMoving", isMoving);
        }
    }

    private void OnDamaged()
    {
        if (isDead) return;

        // Si aún tiene vida tras el golpe, reproduce impacto
        if (health != null && health.CurrentHealth > 0)
        {
            StartCoroutine(PlayHitCoroutine());
        }
    }

    private void OnDeath()
    {
        if (isDead) return;
        isDead = true;

        StartCoroutine(PlayDeathCoroutine());
    }

    private IEnumerator PlayHitCoroutine()
    {
        if (isHitPlaying) yield break;
        isHitPlaying = true;

        if (animator != null) animator.enabled = false;

        if (hitSprites != null && hitSprites.Length > 0 && spriteRenderer != null)
        {
            float frameTime = hitDuration / hitSprites.Length;
            for (int i = 0; i < hitSprites.Length; i++)
            {
                spriteRenderer.sprite = hitSprites[i];
                yield return new WaitForSeconds(frameTime);
            }
        }
        else
        {
            yield return new WaitForSeconds(hitDuration);
        }

        isHitPlaying = false;

        if (!isDead && animator != null)
        {
            animator.enabled = true;
        }
    }

    private IEnumerator PlayDeathCoroutine()
    {
        // 1. Inmovilizar, apagar linterna y desactivar interacciones
        if (weapon != null) weapon.enabled = false;
        if (visionCone != null) visionCone.SetActive(false);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null) animator.enabled = false;

        // 2. Reproducir animación de impacto mortal (daño)
        if (hitSprites != null && hitSprites.Length > 0 && spriteRenderer != null)
        {
            float frameTime = hitDuration / hitSprites.Length;
            for (int i = 0; i < hitSprites.Length; i++)
            {
                spriteRenderer.sprite = hitSprites[i];
                yield return new WaitForSeconds(frameTime);
            }
        }
        else
        {
            yield return new WaitForSeconds(hitDuration);
        }

        // 3. Cambiar al sprite de muerto y dejar el cuerpo en el suelo
        if (spriteRenderer != null && deadSprite != null)
        {
            spriteRenderer.sprite = deadSprite;
            spriteRenderer.sortingOrder = 0; // Al ras del suelo
        }

        // 4. Desactivar el script AI para que no procese más lógica
        enabled = false;
    }

    void FixedUpdate()
    {
        if (isDead || isHitPlaying) return;

        if (moveVelocity.sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
        }
    }
}