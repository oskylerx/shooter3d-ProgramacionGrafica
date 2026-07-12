using UnityEngine;

public class ProyectilVomito : MonoBehaviour
{
    [SerializeField] private int dano = 1;
    [SerializeField] private float tiempoVida = 5f;

    private Vector3 direccionVuelo;
    private float velocidad = 12f;

    void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    public void Inicializar(Vector3 direccion, float velocidadProyectil = 12f)
    {
        direccionVuelo = direccion.normalized;
        velocidad = velocidadProyectil;
    }

    void Update()
    {
        transform.position += direccionVuelo * velocidad * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignorar otros enemigos y sus proyectiles
        if (other.GetComponentInParent<VidaEnemigo>() != null) return;

        // Dañar y ralentizar al jugador
        if (other.CompareTag("Player") || other.GetComponent<Vida>() != null || other.GetComponentInParent<Vida>() != null)
        {
            Vida vidaJugador = other.GetComponent<Vida>() ?? other.GetComponentInParent<Vida>();
            if (vidaJugador != null)
            {
                vidaJugador.RecibirDano(dano);

                // Aplicar ralentización: reducir velocidad del jugador al 30% por 1 segundo
                PrimeraPersona movimiento = other.GetComponent<PrimeraPersona>() 
                                        ?? other.GetComponentInParent<PrimeraPersona>();
                if (movimiento != null)
                {
                    movimiento.Ralentizar(1f);
                    Debug.Log("[ProyectilVomito] Jugador ralentizado 1s.");
                }
            }
            Destroy(gameObject);
            return;
        }

        // Destruirse al chocar con paredes u otros colisionadores sólidos
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
