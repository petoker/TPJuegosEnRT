using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireForce = 20f;
    public float fireRate = 0.5f;
    private float nextFireTime;
    
    [Header("Efectos")]
    public float shakeIntensity = 0.3f;
    public float shakeDuration = 0.1f;

    public void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            
            // Instanciar la bala
            GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // Asignar quién disparó para evitar autodaño
            Projectile projectileScript = projectileObj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.shooter = transform.root.gameObject;
            }

            // Aplicar fuerza a la bala
            Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(firePoint.right * fireForce, ForceMode2D.Impulse);
            }

            // --- NUEVO: Añadir Camera Shake al disparar ---
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ShakeCamera(shakeIntensity, shakeDuration);
            }
        }
    }
}
