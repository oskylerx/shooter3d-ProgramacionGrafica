using UnityEngine;

public class PrimeraPersona : MonoBehaviour
{
    public float velocidad = 5f;
    public float sensibilidad = 2f;
    public float gravedad = -9.81f;
    public Transform camara;

    private CharacterController cc;
    private float pitch = 0f;
    private Vector3 velY;
    private Vector3 knockback = Vector3.zero;

    // Sistema de ralentización
    private float velocidadActual;
    private Coroutine corrutinaRalentizar;
    [SerializeField] private float factorRalentizacion = 0.3f; // 30% de la velocidad normal

    void Start()
    {
        cc = GetComponent<CharacterController>();
        velocidadActual = velocidad;
        // El cursor lo gestiona GameManager; aquí no lo forzamos
    }

    void Update()
    {
        // Si el cursor está visible (menú o game over), no procesar input del jugador
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Decaimiento del efecto de rebote
        knockback = Vector3.Lerp(knockback, Vector3.zero, Time.deltaTime * 5f);

        //Mirar con el raton
        float mx = Input.GetAxis("Mouse X") * sensibilidad;
        float my = Input.GetAxis("Mouse Y") * sensibilidad;
        transform.Rotate(0, mx, 0); // girar el cuerpo
        pitch = Mathf.Clamp(pitch - my, -80f, 80f); // mirar arriba y abajo
        camara.localEulerAngles = new Vector3(pitch, 0, 0);

        // Caminar (WASD o Flechas)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 mov = (transform.right * h + transform.forward * v).normalized * velocidadActual;

        // Gravedad simple
        if (cc.isGrounded && velY.y < 0) velY.y = -2f;
        velY.y += gravedad * Time.deltaTime;

        // Aplicamos el movimiento del jugador, la gravedad y el empuje de rebote
        cc.Move((mov + velY + knockback) * Time.deltaTime);
    }

    // Método para ser empujado hacia atrás por los enemigos
    public void AplicarRebote(Vector3 direccionEmpuje, float fuerza)
    {
        knockback = direccionEmpuje.normalized * fuerza;
    }

    // Ralentizar al jugador durante 'duracion' segundos (efecto del vómito)
    public void Ralentizar(float duracion = 1f)
    {
        if (corrutinaRalentizar != null)
            StopCoroutine(corrutinaRalentizar);
        corrutinaRalentizar = StartCoroutine(EfectoRalentizacion(duracion));
    }

    private System.Collections.IEnumerator EfectoRalentizacion(float duracion)
    {
        velocidadActual = velocidad * factorRalentizacion;
        Debug.Log("[PrimeraPersona] Ralentizado " + duracion + "s (vel=" + velocidadActual + ")");
        yield return new WaitForSeconds(duracion);
        velocidadActual = velocidad;
        Debug.Log("[PrimeraPersona] Velocidad restaurada.");
    }
}
