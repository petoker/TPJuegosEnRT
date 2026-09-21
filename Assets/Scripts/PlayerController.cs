using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Referencias Visuales (Hotline Miami Style)")]
    [Tooltip("Transform del cuerpo/torso que rotarÃ¡ apuntando al mouse")]
    public Transform bodyTransform;
    [Tooltip("Transform de las piernas que rotarÃ¡n apuntando a la direcciÃ³n de movimiento")]
    public Transform legsTransform;
    [Tooltip("SpriteRenderer de las piernas para cambiar sprites de animaciÃ³n")]
    public SpriteRenderer legsRenderer;
    [Tooltip("Animator opcional de las piernas (si se usa Animator Controller)")]
    public Animator legsAnimator;

    [Header("AnimaciÃ³n de Piernas")]
    public Sprite idleLegsSprite;
    public Sprite[] walkLegsSprites;
    public float animationFps = 16f;

    [Header("Ajustes de RotaciÃ³n")]
    [Tooltip("Offset angular en grados si el sprite original mira hacia abajo (por defecto 90)")]
    public float bodyRotationOffset = 90f;
    [Tooltip("Offset angular en grados para las piernas (por defecto 90)")]
    public float legsRotationOffset = 90f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera cam;
    private Weapon playerWeapon;
    private float animTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-detecciÃ³n de referencias si no fueron asignadas en el Inspector
        if (bodyTransform == null)
        {
            Transform b = transform.Find("Body");
            bodyTransform = (b != null) ? b : transform;
        }

        if (legsTransform == null)
        {
            Transform l = transform.Find("Legs");
            if (l != null)
            {
                legsTransform = l;
            }
        }

        if (legsRenderer == null && legsTransform != null)
        {
            legsRenderer = legsTransform.GetComponent<SpriteRenderer>();
        }

        if (legsAnimator == null && legsTransform != null)
        {
            legsAnimator = legsTransform.GetComponent<Animator>();
        }
    }

    void Start()
    {
        cam = Camera.main;
        playerWeapon = GetComponentInChildren<Weapon>();
    }

    // Llamado por el componente PlayerInput (AcciÃ³n: "Move")
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    // Llamado por el componente PlayerInput (AcciÃ³n: "Attack")
    public void OnAttack(InputValue value)
    {
        if (value.isPressed && playerWeapon != null)
        {
            playerWeapon.Shoot();
        }
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        // 1. Apuntado del Cuerpo hacia el mouse (sin animaciÃ³n de caminata)
        RotateBodyTowardsMouse();

        // 2. RotaciÃ³n y animaciÃ³n de las piernas segÃºn la direcciÃ³n de movimiento
        UpdateLegs();
    }

    private void RotateBodyTowardsMouse()
    {
        if (Mouse.current == null || cam == null) return;

        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(screenMousePos);

        Vector3 targetPivot = (bodyTransform != null) ? bodyTransform.position : transform.position;
        Vector2 lookDir = mouseWorldPos - targetPivot;

        if (lookDir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + bodyRotationOffset;
            if (bodyTransform != null && bodyTransform != transform)
            {
                bodyTransform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void UpdateLegs()
    {
        bool isMoving = movement.sqrMagnitude > 0.001f;

        if (isMoving)
        {
            // Rotar las piernas hacia donde se estÃ¡ moviendo el jugador
            float moveAngle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg + legsRotationOffset;
            if (legsTransform != null)
            {
                legsTransform.rotation = Quaternion.Euler(0, 0, moveAngle);
            }

            // AnimaciÃ³n: pasar parÃ¡metro a Animator si existe
            if (legsAnimator != null)
            {
                legsAnimator.SetBool("isMoving", true);
            }

            // AnimaciÃ³n por cÃ³digo (cambio de sprites por frames)
            if (walkLegsSprites != null && walkLegsSprites.Length > 0 && legsRenderer != null)
            {
                animTimer += Time.deltaTime * animationFps;
                int frameIndex = (int)animTimer % walkLegsSprites.Length;
                legsRenderer.sprite = walkLegsSprites[frameIndex];
            }
        }
        else
        {
            // Jugador quieto (Idle)
            if (legsAnimator != null)
            {
                legsAnimator.SetBool("isMoving", false);
            }

            if (legsRenderer != null && idleLegsSprite != null)
            {
                legsRenderer.sprite = idleLegsSprite;
            }

            animTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        // Movimiento fÃ­sico
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}