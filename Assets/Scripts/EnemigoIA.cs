using UnityEngine;
using UnityEngine.AI;

public class EnemigoIA : MonoBehaviour
{
    [Header("Configuración de IA")]
    public float distanciaDeteccion = 20f;

    [Header("Daño por Contacto")]
    public int danoContacto = 1;
    public float intervaloContacto = 0.8f;   // segundos entre cada golpe al tocar
    public float distanciaContacto = 1.8f;   // distancia horizontal considerada "contacto"

    private Transform jugador;
    private NavMeshAgent agent;
    private float proximoGolpe = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Forzar al NavMeshAgent a avanzar hasta tocar al jugador
        if (agent != null)
        {
            agent.stoppingDistance = 0.8f;
        }

        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj == null)
            jugadorObj = GameObject.Find("Jugador");

        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            Debug.Log("[EnemigoIA] " + gameObject.name + ": Jugador encontrado.");
        }
        else
        {
            Debug.LogError("[EnemigoIA] " + gameObject.name + ": NO SE ENCONTRÓ AL JUGADOR.");
        }
    }

    void Update()
    {
        if (jugador == null) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // Calcular distancia horizontal en el plano XZ para ignorar diferencias de altura (Y)
        Vector3 posEnemigoXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posJugadorXZ = new Vector3(jugador.position.x, 0f, jugador.position.z);
        float distanciaXZ = Vector3.Distance(posEnemigoXZ, posJugadorXZ);

        if (distanciaXZ <= distanciaDeteccion)
        {
            // Perseguir al jugador
            agent.isStopped = false;
            agent.SetDestination(jugador.position);

            // Daño al estar en contacto
            if (distanciaXZ <= distanciaContacto && Time.time >= proximoGolpe)
            {
                proximoGolpe = Time.time + intervaloContacto;
                GolpearJugador();
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void GolpearJugador()
    {
        Vida vidaJugador = jugador.GetComponent<Vida>();
        if (vidaJugador == null)
            vidaJugador = jugador.GetComponentInChildren<Vida>();

        if (vidaJugador != null)
        {
            vidaJugador.RecibirDano(danoContacto);
            Debug.Log("[EnemigoIA] " + gameObject.name + " golpeó al jugador. Vida restante: " + vidaJugador.VidaActual());

            // Aplicar rebote al jugador (knockback físico hacia atrás)
            var controller = jugador.GetComponent<PrimeraPersona>();
            if (controller != null)
            {
                // Vector del enemigo al jugador
                Vector3 dirEmpuje = (jugador.position - transform.position).normalized;
                dirEmpuje.y = 0f; // Fuerza puramente horizontal
                controller.AplicarRebote(dirEmpuje, 24f); // Fuerza de 24 para rebote notable
            }
        }
    }
}
