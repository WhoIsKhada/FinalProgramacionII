using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Jugador")]
    public int vidasMaximas = 5;
    public int vidas = 5;

    [Header("Configuración de Daño")]
    public float invulnerabilityTime = 0.5f; // Tiempo de invulnerabilidad después de recibir daño
    private float lastDamageTime = -999f;

    [Header("Puntaje")]
    public int puntaje = 0;
    public int puntajeVictoria = 1500;

    [Header("UI HUD")]
    public TMP_Text textoPuntaje;
    public TMP_Text textoVidas;

    [Header("Pantallas")]
    public GameObject PanelVictoria;
    public GameObject PanelDerrota;
    public Button botonReiniciar;
    public GameObject canvasHUD;

    [Header("Audio (Opcional)")]
    public AudioClip sonidoDanio;
    public AudioClip sonidoGameOver;
    public AudioClip sonidoVictoria;
    private AudioSource audioSource;

    private bool juegoTerminado = false;

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        vidas = vidasMaximas;
        puntaje = 0;
        juegoTerminado = false;

        ActualizarUI();

        if (PanelVictoria != null)
            PanelVictoria.SetActive(false);

        if (PanelDerrota != null)
            PanelDerrota.SetActive(false);

        if (botonReiniciar != null)
            botonReiniciar.onClick.AddListener(ReiniciarJuego);

        // Asegurar que el cursor esté bloqueado al inicio
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SumarPuntos(int cantidad)
    {
        if (juegoTerminado) return;

        puntaje += cantidad;

        ActualizarUI();

        Debug.Log("✅ +" + cantidad + " puntos. Total: " + puntaje);

        if (puntaje >= puntajeVictoria)
            Ganar();
    }

    public void RestarVida(int cantidad = 1)
    {
        if (juegoTerminado) return;

        // Sistema de invulnerabilidad para evitar múltiples daños simultáneos
        if (Time.time - lastDamageTime < invulnerabilityTime)
        {
            Debug.Log("⚠️ Jugador invulnerable");
            return;
        }

        lastDamageTime = Time.time;
        vidas -= cantidad;

        if (vidas < 0)
            vidas = 0;

        ActualizarUI();

        // Reproducir sonido de daño
        if (sonidoDanio != null && audioSource != null)
            audioSource.PlayOneShot(sonidoDanio);

        Debug.Log("💔 -" + cantidad + " vida(s). Vidas restantes: " + vidas);

        if (vidas <= 0)
            Perder();
    }

    public void CurarVida(int cantidad = 1)
    {
        if (juegoTerminado) return;

        vidas += cantidad;

        if (vidas > vidasMaximas)
            vidas = vidasMaximas;

        ActualizarUI();

        Debug.Log("💚 +" + cantidad + " vida(s). Vidas: " + vidas);
    }

    void ActualizarUI()
    {
        if (textoPuntaje != null)
            textoPuntaje.text = "Puntaje: " + puntaje;

        if (textoVidas != null)
            textoVidas.text = "Vidas: " + vidas;
    }

    void Ganar()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (canvasHUD != null)
            canvasHUD.SetActive(false);

        if (PanelVictoria != null)
            PanelVictoria.SetActive(true);

        // Reproducir sonido de victoria
        if (sonidoVictoria != null && audioSource != null)
            audioSource.PlayOneShot(sonidoVictoria);

        Debug.Log("🎉 ¡VICTORIA!");
    }

    void Perder()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (canvasHUD != null)
            canvasHUD.SetActive(false);

        if (PanelDerrota != null)
            PanelDerrota.SetActive(true);

        // Reproducir sonido de game over
        if (sonidoGameOver != null && audioSource != null)
            audioSource.PlayOneShot(sonidoGameOver);

        Debug.Log("💀 GAME OVER");
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
