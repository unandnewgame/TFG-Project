using UnityEngine;
using UnityEngine.SceneManagement; 

public class UIManager : MonoBehaviour
{
    // === SINGLETON PATTERN ===
    // Instance statik agar bisa diakses dari skrip mana pun (Contoh: UIManager.Instance.SetGameState)
    public static UIManager Instance { get; private set; } 
    public static bool IsGamePaused = false; // Static agar bisa diakses dari mana saja

    // Variabel yang akan dihubungkan di Inspector
    public GameObject MainMenuPanel;
    public GameObject PauseMenuPanel;

    // Enum untuk status permainan
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused
    }
    
    private void Awake()
    {
        // Pastikan hanya ada satu instance UIManager (pola Singleton)
        if (Instance == null)
        {
            Instance = this;
            // AKTIF: Membuat objek ini tidak terhapus saat pindah Scene
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            // Jika instance sudah ada, hancurkan objek ini
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Jika di Scene Game (2_GameScene), pastikan waktu berjalan normal
        if (SceneManager.GetActiveScene().name == "2_GameScene") 
        {
            SetGameState(GameState.Playing); 
        }
        else
        {
             SetGameState(GameState.MainMenu);
        }
    }
    
    // Fungsi utama untuk mengatur keadaan permainan
    public void SetGameState(GameState newState)
    {
        // Nonaktifkan semua panel UI utama
        if (MainMenuPanel != null) MainMenuPanel.SetActive(false);
        // Pause Menu harus ada, karena UIManager dibawa ke scene game
        if (PauseMenuPanel != null) PauseMenuPanel.SetActive(false); 
        
        // Reset status sebelum mengatur yang baru
        IsGamePaused = false;
        Time.timeScale = 1f; 

        switch (newState)
        {
            case GameState.MainMenu:
                if (MainMenuPanel != null) MainMenuPanel.SetActive(true);
                break;
            
            case GameState.Playing:
                // Tidak ada panel yang diaktifkan, waktu normal (1f)
                break;

            case GameState.Paused:
                if (PauseMenuPanel != null) PauseMenuPanel.SetActive(true);
                IsGamePaused = true;
                Time.timeScale = 0f; // HENTIKAN WAKTU PERMAINAN
                Debug.Log("Game Paused: TimeScale = 0"); // Pesan konfirmasi pause
                break;
        }
    }

    // Dipanggil oleh PlayButton
    public void PlayGame()
    {
        SetGameState(GameState.Playing); // Pastikan waktu kembali normal sebelum load scene
        SceneManager.LoadScene("2_GameScene"); // Memuat Scene Game
    }

    // Dipanggil oleh ResumeButton di Pause Menu
    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
    }

    // Fungsi STATIK yang dipanggil oleh tombol 'P' dari skrip lain (Tanpa FindObjectOfType!)
    public static void PauseGameToggle()
    {
        // Memastikan Instance sudah tersedia dan panggil fungsi non-statik melalui Instance
        if (Instance != null)
        {
            if (IsGamePaused)
            {
                Instance.ResumeGame();
            }
            else
            {
                Instance.SetGameState(GameState.Paused);
            }
        }
        else
        {
            Debug.LogError("UIManager Instance tidak ditemukan. Pastikan GameObject UIManager ada di Scene.");
        }
    }

    // Dipanggil oleh tombol Quit
    public void QuitGame()
    {
        Debug.Log("Keluar dari Aplikasi.");
        // Kode untuk keluar dari aplikasi (berbeda di Editor vs Build)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #else
        Application.Quit();
        #endif
    }
    
    // Dipanggil oleh tombol Main Menu di Pause Menu
    public void GoToMainMenu()
    {
        SetGameState(GameState.Playing); // Reset waktu sebelum pindah scene
        SceneManager.LoadScene("1_MainMenuScene"); // Memuat Scene Menu
    }
}