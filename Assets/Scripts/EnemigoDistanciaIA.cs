using UnityEngine;
using UnityEngine.AI;

public class EnemigoDistanciaIA : MonoBehaviour
{
    [Header("Configuración de IA")]
    public float distanciaDeteccion = 20f;
    public float distanciaAtaque = 10f;
    public float cadenciaDisparo = 1.5f;

    [Header("Ataque")]
    public GameObject prefabProyectil;
    [SerializeField] private float velocidadProyectil = 18f; // más rápido = más difícil de esquivar

    private Transform jugador;
    private NavMeshAgent agent;
    private float proximoDisparo = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.stoppingDistance = distanciaAtaque - 1f;
        }

        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj == null)
            jugadorObj = GameObject.Find("Jugador");

        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            Debug.Log("[EnemigoDistanciaIA] " + gameObject.name + ": Jugador encontrado.");
        }
        else
        {
            Debug.LogError("[EnemigoDistanciaIA] " + gameObject.name + ": NO SE ENCONTRÓ AL JUGADOR.");
        }
    }

    void Update()
    {
        if (jugador == null) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        Vector3 posEnemigoXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posJugadorXZ = new Vector3(jugador.position.x, 0f, jugador.position.z);
        float distanciaXZ = Vector3.Distance(posEnemigoXZ, posJugadorXZ);

        if (distanciaXZ <= distanciaDeteccion)
        {
            if (distanciaXZ > distanciaAtaque)
            {
                // Acercarse hasta estar en rango de ataque
                agent.isStopped = false;
                agent.SetDestination(jugador.position);
            }
            else
            {
                // En rango: detenerse, encarar al jugador y disparar
                agent.isStopped = true;

                // Orientar la cápsula hacia el jugador (solo eje horizontal)
                Vector3 objetivoLook = new Vector3(jugador.position.x, transform.position.y, jugador.position.z);
                transform.LookAt(objetivoLook);

                if (Time.time >= proximoDisparo)
                {
                    proximoDisparo = Time.time + cadenciaDisparo;
                    DispararProyectil();
                }
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void DispararProyectil()
    {
        if (prefabProyectil == null)
        {
            Debug.LogWarning("[EnemigoDistanciaIA] Prefab de proyectil no asignado.");
            return;
        }

        // Origen: ligero offset para que salga desde la "boca" del enemigo,
        // pero a la misma altura relativa del cuerpo (no desde la cabeza)
        Vector3 origen = transform.position + Vector3.up * 0.3f;

        // Destino: jugador.position ya ES el centro del CharacterController
        // (center=(0,0,0) significa que el pivot está en el centro del capsule).
        // NO sumamos offset vertical para no apuntar a la cabeza.
        Vector3 destino = jugador.position;

        Vector3 direccion = (destino - origen).normalized;

        Debug.Log("[EnemigoDistanciaIA] Disparando hacia " + destino + " dir:" + direccion);

        GameObject projGo = Instantiate(prefabProyectil, origen, Quaternion.LookRotation(direccion));
        ProyectilVomito projScript = projGo.GetComponent<ProyectilVomito>();
        if (projScript != null)
        {
            projScript.Inicializar(direccion, velocidadProyectil);
        }
    }
}
