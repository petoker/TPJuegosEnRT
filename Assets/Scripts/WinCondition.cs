using UnityEngine;

public class WinCondition : MonoBehaviour
{
    // Contador de objetos
    public static int remainingItems = 0;

    public GameObject victoryScreen;

    private bool playerInRange = false;

    void Awake()
    {
        remainingItems++;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }
    private void Collect()
    {
        remainingItems--;

        if (remainingItems <= 0)
        {
            if (victoryScreen != null)
            {
                victoryScreen.SetActive(true);
            }
            Time.timeScale = 0f;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
