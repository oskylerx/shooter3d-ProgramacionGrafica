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
    [SerializeField] private float velocidadProyectil = 12f;

    private Transform jugador;
    private NavMeshAgent agent;
    private float proximoDisparo = 0f;

    // Variables para predecir la posición futura del jugador
    private Vector3 posJugadorAnterior;
    private Vector3 velocidadJugador;

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
            posJugadorAnterior = jugador.position;
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

        // Calcular velocidad real del jugador en este frame
        velocidadJugador = (jugador.position - posJugadorAnterior) / Time.deltaTime;
        posJugadorAnterior = jugador.position;

        Vector3 posEnemigoXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 posJugadorXZ = new Vector3(jugador.position.x, 0f, jugador.position.z);
        float distanciaXZ = Vector3.Distance(posEnemigoXZ, posJugadorXZ);

        if (distanciaXZ <= distanciaDeteccion)
        {
            if (distanciaXZ > distanciaAtaque)
            {
                agent.isStopped = false;
                agent.SetDestination(jugador.position);
            }
            else
            {
                agent.isStopped = true;

                // Mirar hacia la posición predicha del jugador
                Vector3 objetivoLook = jugador.position;
                objetivoLook.y = transform.position.y;
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

        Vector3 origen = transform.position + Vector3.up * 1.5f;
        Vector3 posActualJugador = jugador.position + Vector3.up * 1.0f;

        // Predicción: calcular tiempo de vuelo a la posición actual y
        // compensar con la velocidad del jugador en ese tiempo
        float distancia = Vector3.Distance(origen, posActualJugador);
        float tiempoVuelo = distancia / velocidadProyectil;

        // Posición predicha donde estará el jugador cuando llegue el proyectil
        Vector3 posPredicha = posActualJugador + velocidadJugador * tiempoVuelo;
        // Ignorar componente vertical para no mandar el proyectil al suelo ni al cielo
        posPredicha.y = posActualJugador.y;

        Vector3 direccion = (posPredicha - origen).normalized;

        GameObject projGo = Instantiate(prefabProyectil, origen, Quaternion.LookRotation(direccion));
        ProyectilVomito projScript = projGo.GetComponent<ProyectilVomito>();
        if (projScript != null)
        {
            projScript.Inicializar(direccion, velocidadProyectil);
        }
    }
}
