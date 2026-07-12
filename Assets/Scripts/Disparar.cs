using System.Collections;
using UnityEngine;
using TMPro;

public class Disparar : MonoBehaviour
{
    [Header("Referencias de Disparo")]
    public Camera camara;
    public int dano = 2;
    public float alcance = 100f;
    public float cadencia = 0.1f;
    public AudioClip sonidoDisparo;
    public GameObject muzzle;

    [Header("Sistema de Munición")]
    [SerializeField] private int capacidadCargador = 10;
    [SerializeField] private int municionActual;
    [SerializeField] private float tiempoRecarga = 1.5f;
    [SerializeField] private TextMeshProUGUI textoMunicion;

    private AudioSource fuente;
    private float proximo = 0f;
    private bool estaRecargando = false;

    void Start()
    {
        fuente = GetComponent<AudioSource>();
        if (muzzle != null) muzzle.SetActive(false);

        // Inicializamos la munición al máximo al comenzar
        municionActual = capacidadCargador;
        ActualizarTextoMunicion();
    }

    void Update()
    {
        // Detectar recarga manual
        if (Input.GetKeyDown(KeyCode.R))
        {
            IniciarRecarga();
        }

        // Detectar disparo
        if (Input.GetMouseButtonDown(0) && Time.time >= proximo)
        {
            if (!estaRecargando && municionActual > 0)
            {
                proximo = Time.time + cadencia;
                Disparo();
            }
        }
    }

    void Disparo()
    {
        // Restar munición y actualizar UI
        municionActual--;
        ActualizarTextoMunicion();

        if (sonidoDisparo != null) fuente.PlayOneShot(sonidoDisparo);
        
        if (muzzle != null) 
        { 
            muzzle.SetActive(true);
            CancelInvoke("ApagarMuzzle"); // Cancelar invocaciones previas para evitar conflictos de cadencia rápida
            Invoke("ApagarMuzzle", 0.05f);
        }

        Ray ray = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        // Usar RaycastAll para ignorar colisiones con el propio jugador al disparar muy cerca
        RaycastHit[] hits = Physics.RaycastAll(ray, alcance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            // Ignorar el colisionador del propio jugador (o cualquier objeto en su jerarquía)
            if (hit.transform.CompareTag("Player") || hit.transform.gameObject == gameObject || hit.transform.IsChildOf(transform.root))
            {
                continue;
            }

            // Primero comprobamos si impactó a un enemigo
            VidaEnemigo enemigo = hit.collider.GetComponentInParent<VidaEnemigo>();
            if (enemigo != null)
            {
                enemigo.RecibirDano(1);
            }
            else
            {
                // Si no es un enemigo, comprobamos Vida (otros objetos destructibles)
                Vida v = hit.collider.GetComponentInParent<Vida>();
                if (v != null) v.RecibirDano(dano);
            }

            // Detener el rayo en el primer obstáculo sólido impactado
            break;
        }
    }

    void ApagarMuzzle()
    {
        if (muzzle != null)
        {
            muzzle.SetActive(false);
        }
    }

    public void IniciarRecarga()
    {
        // Solo recarga si no está recargando ya y si le faltan balas
        if (!estaRecargando && municionActual < capacidadCargador)
        {
            StartCoroutine(Recargar());
        }
    }

    private IEnumerator Recargar()
    {
        estaRecargando = true;
        
        if (textoMunicion != null)
        {
            textoMunicion.text = "Recargando...";
        }

        yield return new WaitForSeconds(tiempoRecarga);

        municionActual = capacidadCargador;
        estaRecargando = false;
        
        ActualizarTextoMunicion();
    }

    private void ActualizarTextoMunicion()
    {
        if (textoMunicion != null)
        {
            textoMunicion.text = "Munición: " + municionActual + " / " + capacidadCargador;
        }
    }
}
