using UnityEngine;
using UnityEngine.SceneManagement;

public class Vida : MonoBehaviour
{
    public int vidaMax = 3;
    public bool esJugador = false;
    private int vidaActual;

    void Start()
    {
        vidaActual = vidaMax;
        
        // Autodetectar si este objeto es el jugador basándose en el tag o el nombre
        if (CompareTag("Player") || gameObject.name == "Jugador" || GetComponent<PrimeraPersona>() != null)
        {
            esJugador = true;
        }
    }

    public void RecibirDano(int cantidad)
    {
        vidaActual -= cantidad;
        
        // Si es el jugador, activar el filtro de daño en pantalla
        if (esJugador && GameManager.Instance != null)
        {
            GameManager.Instance.MostrarFiltroRojo();
        }

        if (vidaActual <= 0) Morir();
    }

    void Morir()
    {
        if (esJugador)
        {
            // Notificar al GameManager para mostrar el panel Game Over
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); // fallback
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int VidaActual()
    {
        return vidaActual;
    }

    // Recuperar vida sin superar el máximo
    public void Curar(int cantidad)
    {
        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMax);
    }
}
