using UnityEngine;
using System.Collections;

public class VidaEnemigo : MonoBehaviour
{
    [SerializeField] private int vidaMax = 3;
    private int vidaActual;
    private bool estaMuerto = false;

    private Renderer rend;
    private Color colorOriginal;
    private Coroutine corrutinaDano;

    void Start()
    {
        vidaActual = vidaMax;
        
        // Obtener el componente Renderer del enemigo para poder cambiar su color
        rend = GetComponent<Renderer>();
        if (rend == null) 
            rend = GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            // Guardamos el color original del material instanciado
            colorOriginal = rend.material.color;
        }
    }

    public void RecibirDano(int dano)
    {
        if (estaMuerto) return;

        vidaActual -= dano;

        // Feedback de daño: Parpadeo en color rojo
        if (rend != null)
        {
            if (corrutinaDano != null) 
                StopCoroutine(corrutinaDano);
            
            corrutinaDano = StartCoroutine(FlashRojo());
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private IEnumerator FlashRojo()
    {
        // Cambiar a color rojo brillante
        rend.material.color = Color.red;
        
        // Mantenerlo durante 0.12 segundos (120 milisegundos)
        yield return new WaitForSeconds(0.12f);
        
        // Restaurar el color original del enemigo
        if (rend != null)
        {
            rend.material.color = colorOriginal;
        }
    }

    private void Morir()
    {
        estaMuerto = true;

        // Desactivamos la IA y navegación para evitar acciones post-mortem
        EnemigoIA ia = GetComponent<EnemigoIA>();
        if (ia != null) ia.enabled = false;

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Destruimos el GameObject del enemigo
        Destroy(gameObject);
    }
}
