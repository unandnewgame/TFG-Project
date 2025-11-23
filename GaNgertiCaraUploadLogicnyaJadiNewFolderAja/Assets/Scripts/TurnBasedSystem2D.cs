using UnityEngine;
using System.Collections;
using TMPro;

public class TurnBasedSystem2D : MonoBehaviour
{
    // Variabel yang akan dihubungkan di Unity Editor
    public Transform player;// Pemain 
    public Transform enemy;// Musuh
    public Transform playerStartPosition;// Posisi awal pemain
    public GameObject turnIndicatorPrefab;// Prefab Cincin Indikator Giliran
    public TextMeshProUGUI gameOverText;// Teks Game Over
    
    // Variabel internal
    private Vector3 _enemyStartPosition;// Simpan posisi awal musuh

    private CharacterStats playerStats;// Statistik Pemain
    private CharacterStats enemyStats;// Statistik Musuh

    private bool isPlayerTurn = true;// Menandai giliran pemain
    private bool isExecutingAction = false;// Menandai apakah karakter sedang melakukan aksi
    private float attackSpeed = 4f; // Kecepatan pergerakan saat menyerang
    public float attackDistanceOffset = 1.0f; // Jarak berhenti dari target
    private GameObject currentTurnIndicator;// Indikator Giliran Saat Ini

    private void StartTurn(CharacterStats character)
    {
        // 1. Hapus Indicator Lama (jika ada)
        if (currentTurnIndicator != null)
        {
            Destroy(currentTurnIndicator);
        }

        // 2. Buat Indicator Baru (Cincin) dan Jadikan Karakter sebagai Parent
        if (turnIndicatorPrefab != null)
        {
            // Buat Indicator dan atur parent ke karakter yang mendapat giliran.
            currentTurnIndicator = Instantiate(turnIndicatorPrefab, character.transform);
            
            // Atur posisi lokal (local position) cincin. 
            // Z=-0.1f (atau nilai negatif kecil) untuk sedikit di belakang karakter.
            currentTurnIndicator.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        }
        isExecutingAction = false;
    }


    // Fungsi untuk menangani kondisi Game Over
    private void GameOver()
    {
        if (currentTurnIndicator != null)// Hapus Indicator yang Tersisa
        {
            Destroy(currentTurnIndicator);
        }
        // Tampilkan Pesan Game Over (Menang/Kalah)
        if (gameOverText != null)
        {
            if (playerStats.isDead)
            {
                gameOverText.text = "ANDA KALAH";
                gameOverText.color = Color.red; 
            }
            else if (enemyStats.isDead)
            {
                gameOverText.text = "ANDA MENANG";
                gameOverText.color = Color.green; 
            }
            // Aktifkan Teks Game Over
            gameOverText.gameObject.SetActive(true);
        }
    }


    void Start()
    {
        if (enemy != null)
        {
            _enemyStartPosition = enemy.position;
            enemyStats = enemy.GetComponent<CharacterStats>();
        }
        
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
        }

        Debug.Log("Game Dimulai! Giliran Pemain. Tekan SPASI untuk menyerang, 'P' untuk pause.");
        if (playerStats != null)
        {
             StartTurn(playerStats);
        }
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false); 
        }
    }


    void Update()
    {
        // 1. Panggil Pause: HANYA jika game TIDAK dalam keadaan pause
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            // Pengecekan baru: Hanya pause jika game sedang Playing
            if (!UIManager.IsGamePaused)
            {
                UIManager.PauseGameToggle(); 
            }
            // Catatan: Untuk unpause, sekarang HANYA bisa dari tombol UI.
        }

        // 2. Cek status pause
        if (UIManager.IsGamePaused)
        {
            return; // Keluar dari Update() segera saat game dijeda
        }

        // --- Logika Giliran Game ---
        if (!isExecutingAction)
        {
            if (playerStats != null && playerStats.isDead || enemyStats != null && enemyStats.isDead) 
            {
                GameOver();
                return;
            }

            if (isPlayerTurn)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartPlayerAttack();
                }
            }
            else
            {
                // Jeda sebentar sebelum musuh menyerang
                StartCoroutine(EnemyAttackRoutine(1.5f)); 
                isExecutingAction = true;
            }
        }
    }

    IEnumerator EnemyAttackRoutine(float delay)
    {
        // Catatan: new WaitForSeconds(delay) juga otomatis berhenti saat Time.timeScale = 0f.
        yield return new WaitForSeconds(delay);
        if (!isPlayerTurn && !enemyStats.isDead) 
        {
            StartEnemyAttack();
        }
    }

    private void StartPlayerAttack()
    {
        isExecutingAction = true;
        Debug.Log("Giliran Pemain: Menyerang!");
        
        StartCoroutine(MoveAndAttack(player, enemy.position, playerStartPosition.position, playerStats.attackDamage, enemyStats, () => 
        {
            if (!enemyStats.isDead)
            {
                isPlayerTurn = false;
                StartTurn(enemyStats);
            }
            else
            {
                GameOver();
            }
            isExecutingAction = false;
        }));
    }

    private void StartEnemyAttack()
    {
        isExecutingAction = true;
        Debug.Log("Giliran Musuh: Menyerang!");
        
        StartCoroutine(MoveAndAttack(enemy, player.position, _enemyStartPosition, enemyStats.attackDamage, playerStats, () => 
        {
            if (!playerStats.isDead)
            {
                isPlayerTurn = true;
                StartTurn(playerStats);
            }
            else
            {
                GameOver();
            }
            isExecutingAction = false;
        }));
    }

    // Coroutine yang menangani pergerakan, animasi, dan damage
    IEnumerator MoveAndAttack(Transform attacker, Vector3 targetPos, Vector3 returnPos, int damage, CharacterStats targetStats, System.Action onComplete)
    {
        Animator attackerAnim = attacker.GetComponent<Animator>(); // Ambil Animator Attacker
        
        // --- HITUNG POSISI BERHENTI (OFFSET) ---
        Vector3 direction = (targetPos - attacker.position).normalized; // Cari arah
        Vector3 stopPos = targetPos - (direction * attackDistanceOffset); // Hitung posisi berhenti

        if (Vector3.Distance(attacker.position, stopPos) < 0.1f) 
        {
            stopPos = attacker.position;
        }
        // ---------------------------------------

        // 1. Bergerak Maju ke Posisi Berhenti (stopPos)
        float journeyLength = Vector3.Distance(attacker.position, stopPos); 
        float startTime = Time.time;

        while (Vector3.Distance(attacker.position, stopPos) > 0.1f)
        {
            if (UIManager.IsGamePaused) // Pengecekan PAUSE INSTAN (Memastikan Coroutine berhenti)
            {
               yield return null; 
               continue; // Tetap di loop, menunggu sampai di-resume
            }
            float distCovered = (Time.time - startTime) * attackSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            attacker.position = Vector3.Lerp(attacker.position, stopPos, fractionOfJourney);
            yield return null; 
        }
        
        attacker.position = stopPos; 

        // --- PICU ANIMASI SERANGAN ---
        if (attackerAnim != null)
        {
            attackerAnim.SetTrigger("Attack"); 
        }
        
        yield return new WaitForSeconds(0.4f); 

        // --- LOGIKA DAMAGE ---
        if (targetStats != null && !targetStats.isDead)
        {
            targetStats.TakeDamage(damage);
        }
        
        yield return new WaitForSeconds(0.4f); 
        
        // --- KEMBALI KE IDLE SEBELUM MUNDUR ---
        if (attackerAnim != null)
        {
            attackerAnim.SetTrigger("Idle"); 
        }


        // 2. Bergerak Mundur ke Posisi Semula
        journeyLength = Vector3.Distance(attacker.position, returnPos);
        startTime = Time.time;

        while (Vector3.Distance(attacker.position, returnPos) > 0.1f) // Kembali ke posisi semula
        {
            if (UIManager.IsGamePaused) // Pengecekan PAUSE INSTAN
            {
               yield return null; 
               continue; // Tetap di loop, menunggu sampai di-resume
            }
            float distCovered = (Time.time - startTime) * attackSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            attacker.position = Vector3.Lerp(attacker.position, returnPos, fractionOfJourney);
            yield return null;
        }

        attacker.position = returnPos; 

        onComplete?.Invoke();
    }
}