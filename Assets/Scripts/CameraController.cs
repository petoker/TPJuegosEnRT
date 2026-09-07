using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector3 offset = new Vector3(0, 0, -10f);
    
    [Header("Shake Settings")]
    [Tooltip("Controla qué tan rápido vibra la cámara. Valores altos significan vibraciones más rápidas.")]
    public float shakeFrequency = 35f;
    
    public static CameraController Instance;

    private float currentShakeDuration = 0f;
    private float currentShakeIntensity = 0f;
    private float shakeSeedX;
    private float shakeSeedY;

    private Vector3 velocity;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Si no se asignó un target, buscamos al jugador automáticamente por su tag
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Movimiento suave hacia el objetivo
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

        // Aplicar Camera Shake si está activo
        if (currentShakeDuration > 0)
        {
            float shakeX = (Mathf.PerlinNoise(Time.unscaledTime * shakeFrequency + shakeSeedX, 0f) - 0.5f) * 2f;
            float shakeY = (Mathf.PerlinNoise(0f, Time.unscaledTime * shakeFrequency + shakeSeedY) - 0.5f) * 2f;
            
            // Sumamos el shake a la posición actual calculada por SmoothDamp
            transform.position += new Vector3(shakeX, shakeY, 0f) * currentShakeIntensity;
            
            currentShakeDuration -= Time.unscaledDeltaTime;
        }
    }

    // Llama a esta función para hacer vibrar la cámara (ej: al disparar o al morir)
    // Ejemplo: CameraController.Instance.ShakeCamera(0.5f, 0.2f);
    public void ShakeCamera(float intensity, float duration)
    {
        currentShakeIntensity = intensity;
        currentShakeDuration = duration;
        shakeSeedX = Random.Range(0f, 100f);
        shakeSeedY = Random.Range(0f, 100f);
    }
}
