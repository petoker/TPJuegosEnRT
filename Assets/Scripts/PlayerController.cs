using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera cam;
    private Weapon playerWeapon;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        playerWeapon = GetComponentInChildren<Weapon>();
    }

    // Llamado por el componente PlayerInput (Acción: "Move")
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    // Llamado por el componente PlayerInput (Acción: "Attack")
    public void OnAttack(InputValue value)
    {
        if (value.isPressed && playerWeapon != null)
        {
            playerWeapon.Shoot();
        }
    }

    void Update()
    {
        // Apuntado hacia el mouse usando el nuevo Input System
        if (Mouse.current != null)
        {
            Vector2 screenMousePos = Mouse.current.position.ReadValue();
            Vector3 mousePos = cam.ScreenToWorldPoint(screenMousePos);
            Vector2 lookDir = mousePos - transform.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            
            // Asume que el sprite original mira hacia la derecha. Si mira hacia arriba, resta 90 al ángulo.
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        // Movimiento físico
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
