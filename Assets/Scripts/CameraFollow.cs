using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10f);
    
    [Header("Estilo Hotline Miami (Mirar hacia el mouse)")]
    public bool lookAhead = true;
    public float lookAheadAmount = 3f;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        Vector3 targetPos = target.position;

        if (lookAhead && Mouse.current != null)
        {
            // Usamos la posición del mouse en la pantalla en lugar del mundo
            // para evitar un bucle de retroalimentación donde la cámara y el mundo se empujan mutuamente.
            Vector2 screenMousePos = Mouse.current.position.ReadValue();
            
            // Convertimos la posición de la pantalla a un rango de -1 a 1
            float screenX = (screenMousePos.x / Screen.width - 0.5f) * 2f;
            float screenY = (screenMousePos.y / Screen.height - 0.5f) * 2f;
            
            // Limitamos a un círculo perfecto (para que las esquinas no se alejen más)
            Vector2 mouseOffset = Vector2.ClampMagnitude(new Vector2(screenX, screenY), 1f);
            
            // Aplicamos el multiplicador de distancia
            targetPos.x += mouseOffset.x * lookAheadAmount;
            targetPos.y += mouseOffset.y * lookAheadAmount;
        }

        Vector3 desiredPosition = targetPos + offset;
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
