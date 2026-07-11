using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Paneles UI")]
    public GameObject panelMenu;
    public GameObject panelGameOver;
    public GameObject panelVictoria;

    [Header("Efectos Visuales")]
    public Image filtroRojo;

    [Header("HUD")]
    public TextMeshProUGUI textoHUD;

    private Vida vidaJugador;
    private bool juegoIniciado = false;
    private Coroutine corrutinaFiltro;
    private int totalEnemigosIniciales = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CablearComponentes();

        // Buscar componente de vida del jugador
        var jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null) jugador = GameObject.Find("Jugador");
        if (jugador != null) vidaJugador = jugador.GetComponent<Vida>();

        // Contar enemigos iniciales en la escena
        var enemigos = GameObject.FindObjectsByType<VidaEnemigo>(FindObjectsInactive.Exclude);
        totalEnemigosIniciales = enemigos != null ? enemigos.Length : 0;
        Debug.Log("[GameManager] Enemigos iniciales detectados: " + totalEnemigosIniciales);

        // Mostrar menú al iniciar: pausar tiempo y mostrar cursor
        Time.timeScale = 0f;

        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);

        if (filtroRojo != null)
        {
            Color col = filtroRojo.color;
            col.a = 0f;
            filtroRojo.color = col;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ActualizarHUD();
    }

    void Update()
    {
        if (juegoIniciado)
        {
            ActualizarHUD();
            VerificarCondicionVictoria();
        }
    }

    public void ActualizarHUD()
    {
        if (textoHUD == null) return;

        // 1. Obtener la vida del jugador y formatearla en barritas (mínimo 0)
        int vida = 0;
        if (vidaJugador != null)
        {
            vida = Mathf.Max(0, vidaJugador.VidaActual());
        }

        string barritasVida = "";
        for (int i = 1; i <= 3; i++)
        {
            if (i <= vida)
                barritasVida += "■ ";
            else
                barritasVida += "░ ";
        }

        // 2. Contar enemigos activos en la escena
        var enemigos = GameObject.FindObjectsByType<VidaEnemigo>(FindObjectsInactive.Exclude);
        int cantEnemigos = enemigos != null ? enemigos.Length : 0;

        // 3. Escribir texto en el HUD
        textoHUD.text = $"VIDA: <color=#FF3333>{barritasVida}</color> | ENEMIGOS: <color=#33FF33>{cantEnemigos}</color>";
    }

    private void VerificarCondicionVictoria()
    {
        // Solo evaluar si el juego está activo y había enemigos en el mapa
        if (!juegoIniciado || totalEnemigosIniciales == 0) return;

        var enemigos = GameObject.FindObjectsByType<VidaEnemigo>(FindObjectsInactive.Exclude);
        if (enemigos == null || enemigos.Length == 0)
        {
            Victoria();
        }
    }

    public void Victoria()
    {
        Debug.Log("[GameManager] Victoria()");
        juegoIniciado = false;
        Time.timeScale = 0f;

        // Forzar actualización del HUD para mostrar los datos finales exactos
        ActualizarHUD();

        if (panelVictoria != null) panelVictoria.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GameOver()
    {
        Debug.Log("[GameManager] GameOver()");
        juegoIniciado = false;
        Time.timeScale = 0f;

        // Forzar actualización del HUD para mostrar 0 vidas (░ ░ ░) al morir
        ActualizarHUD();

        if (panelGameOver != null) panelGameOver.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void MostrarFiltroRojo()
    {
        if (filtroRojo == null) return;
        if (corrutinaFiltro != null) StopCoroutine(corrutinaFiltro);
        corrutinaFiltro = StartCoroutine(FiltroRojoFade());
    }

    private IEnumerator FiltroRojoFade()
    {
        float duracion = 0.35f;
        float tiempo = 0f;
        Color col = filtroRojo.color;

        col.a = 0.5f; // Alfa inicial del filtro rojo de daño
        filtroRojo.color = col;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            col.a = Mathf.Lerp(0.5f, 0f, tiempo / duracion);
            filtroRojo.color = col;
            yield return null;
        }

        col.a = 0f;
        filtroRojo.color = col;
    }

    public void Reiniciar()
    {
        Debug.Log("[GameManager] Reiniciar()");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IniciarJuego()
    {
        Debug.Log("[GameManager] IniciarJuego()");
        Time.timeScale = 1f;
        juegoIniciado = true;

        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ActualizarHUD();
    }

    private void CablearComponentes()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("[GameManager] Canvas no encontrado."); return; }

        // Buscar referencias si no están asignadas en el Inspector
        if (panelMenu == null || panelGameOver == null || panelVictoria == null || filtroRojo == null)
        {
            foreach (Transform child in canvas.transform)
            {
                if (child.name == "PanelMenu") panelMenu = child.gameObject;
                if (child.name == "PanelGameOver") panelGameOver = child.gameObject;
                if (child.name == "PanelVictoria") panelVictoria = child.gameObject;
                if (child.name == "FiltroRojo") filtroRojo = child.GetComponent<Image>();
            }
        }

        if (textoHUD == null)
        {
            var hudGo = GameObject.Find("TextoHUD");
            if (hudGo != null) textoHUD = hudGo.GetComponent<TextMeshProUGUI>();
        }

        // Botón Iniciar
        if (panelMenu != null)
        {
            var btnT = panelMenu.transform.Find("BtnIniciar");
            if (btnT != null)
            {
                var btn = btnT.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(IniciarJuego);
                Debug.Log("[GameManager] BtnIniciar conectado.");
            }
        }

        // Botón Reiniciar (Game Over)
        if (panelGameOver != null)
        {
            var btnT = panelGameOver.transform.Find("BtnReiniciar");
            if (btnT != null)
            {
                var btn = btnT.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Reiniciar);
                Debug.Log("[GameManager] BtnReiniciar conectado.");
            }
        }

        // Botón Reiniciar (Victoria)
        if (panelVictoria != null)
        {
            var btnT = panelVictoria.transform.Find("BtnReiniciarVictoria");
            if (btnT != null)
            {
                var btn = btnT.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Reiniciar);
                Debug.Log("[GameManager] BtnReiniciarVictoria conectado.");
            }
        }
    }
}
