using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Objetivo y Movimiento")]
    public Transform target; // El jugador
    public float moveSpeed = 3f;
    public float stoppingDistance = 5f; // Distancia para empezar a disparar

    [Header("AnimaciÃ³n")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Sprite idleSprite;
    public Sprite[] walkSprites;
    public float animationFps = 12f;

    [Header("Efectos de DaÃ±o y Muerte")]
    [Tooltip("Sprite del cuerpo caÃ­do cuando muere")]
    public Sprite deadSprite;
    [Tooltip("Sprites de la animaciÃ³n al recibir impacto/daÃ±o")]
    public Sprite[] hitSprites;
    [Tooltip("DuraciÃ³n de la animaciÃ³n de impacto en segundos")]
    public float hitDuration = 0.2f;

    [Header("Ajustes de RotaciÃ³n")]
    [Tooltip("Offset angular en grados si el sprite original mira hacia abajo (por defecto 90)")]
    public float rotationOffset = 90f;

    private Rigidbody2D rb;
    private Weapon weapon;
    private Health health;
    private Vector2 moveVelocity;
    private float animTimer;
    private bool isDead;
    private bool isHitPlaying;

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

        if (health != null)
        {
            health.OnDamaged.AddListener(OnDamaged);
            health.OnDeath.AddListener(OnDeath);
        }
    }

    void Start()
    {
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

        if (target == null)
        {
            moveVelocity = Vector2.zero;
            UpdateAnimation(false);
            return;
        }

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float distance = toTarget.magnitude;

        if (distance > stoppingDistance)
        {
            // Se mueve hacia el jugador
            Vector2 moveDir = toTarget.normalized;
            moveVelocity = moveDir * moveSpeed;

            // Si no se estÃ¡ reproduciendo el impacto, mirar hacia la direcciÃ³n de movimiento
            if (!isHitPlaying)
            {
                float moveAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg + rotationOffset;
                transform.rotation = Quaternion.Euler(0, 0, moveAngle);
            }

            // Animar ciclo de caminata
            UpdateAnimation(true);
        }
        else
        {
            // Detenido dentro del rango de disparo
            moveVelocity = Vector2.zero;

            // Mirar hacia el jugador para apuntar
            if (!isHitPlaying && distance > 0.0001f)
            {
                float aimAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + rotationOffset;
                transform.rotation = Quaternion.Euler(0, 0, aimAngle);
            }

            // Sprite quieto (idle)
            UpdateAnimation(false);

            // Disparar si tiene un arma
            if (weapon != null && !isHitPlaying)
            {
                weapon.Shoot();
            }
        }
    }

    private void UpdateAnimation(bool isMoving)
    {
        if (isHitPlaying || isDead) return;

        if (animator != null && animator.enabled)
        {
            animator.SetBool("isMoving", isMoving);
        }

        if (spriteRenderer == null) return;

        if (isMoving && walkSprites != null && walkSprites.Length > 0)
        {
            animTimer += Time.deltaTime * animationFps;
            int frame = (int)animTimer % walkSprites.Length;
            spriteRenderer.sprite = walkSprites[frame];
        }
        else
        {
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
            animTimer = 0f;
        }
    }

    private void OnDamaged()
    {
        if (isDead) return;

        // Si aÃºn tiene vida tras el golpe, reproduce impacto
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
        // 1. Inmovilizar y desactivar interacciones
        if (weapon != null) weapon.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null) animator.enabled = false;

        // 2. Reproducir animaciÃ³n de impacto mortal (daÃ±o)
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

        // 4. Desactivar el script AI para que no procese mÃ¡s lÃ³gica
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