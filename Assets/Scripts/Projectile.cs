using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 2f;
    
    [HideInInspector] public GameObject shooter; // Referencia directa a quien disparÃ³

    [Header("AnimaciÃ³n de la Bala")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] animationSprites;
    public float animationFps = 20f;
    public bool loopAnimation = true;

    private float animTimer;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (animationSprites != null && animationSprites.Length > 0 && spriteRenderer != null)
        {
            animTimer += Time.deltaTime * animationFps;
            int frame = (int)animTimer;
            if (loopAnimation)
            {
                spriteRenderer.sprite = animationSprites[frame % animationSprites.Length];
            }
            else
            {
                if (frame < animationSprites.Length)
                {
                    spriteRenderer.sprite = animationSprites[frame];
                }
            }
        }
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
        // Evitar que la bala le haga daÃ±o a quien la disparÃ³
        if (shooter != null && hitObject.transform.IsChildOf(shooter.transform))
        {
            return;
        }

        Debug.Log("La bala chocÃ³ contra: " + hitObject.name);

        Health health = hitObject.GetComponentInParent<Health>();
        if (health != null)
        {
            Debug.Log("Â¡Aplicando daÃ±o a " + health.gameObject.name + "!");
            health.TakeDamage(damage);
        }

        // Destruir la bala al chocar
        Destroy(gameObject);
    }
}