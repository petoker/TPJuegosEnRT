using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Nombre de la escena del juego que se cargará al presionar Jugar")]
    public string gameSceneName = "SampleScene";

    // Asignar esta función al evento OnClick() del botón Jugar
    public void Jugar()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Asignar esta función al evento OnClick() del botón Salir
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
