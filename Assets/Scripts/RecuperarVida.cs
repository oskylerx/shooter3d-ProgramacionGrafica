using UnityEngine;

public class RecuperarVida : MonoBehaviour
{
    [SerializeField] private int vidaRecuperada = 1;
    [SerializeField] private float velocidadRotacion = 90f; // grados por segundo

    void Update()
    {
        // Rotación animada para que sea visible como pickup
        transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<Vida>() != null || other.GetComponentInParent<Vida>() != null)
        {
            Vida vidaJugador = other.GetComponent<Vida>() ?? other.GetComponentInParent<Vida>();
            if (vidaJugador == null) return;

            // Solo curar si le falta vida (evitar desperdiciar el pickup)
            if (vidaJugador.VidaActual() < vidaJugador.vidaMax)
            {
                vidaJugador.Curar(vidaRecuperada);
                Debug.Log("[RecuperarVida] Jugador curado en +" + vidaRecuperada);
                Destroy(gameObject);
            }
        }
    }
}
