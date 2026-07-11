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

    void Start()
    {
        cc = GetComponent<CharacterController>();
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
        Vector3 mov = (transform.right * h + transform.forward * v).normalized * velocidad;

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
}
