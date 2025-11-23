using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{
    // Variabel yang dihubungkan di Unity Editor
    public Slider hpBar;
    public GameObject damageTextPrefab; // Prefab untuk Floating Damage Text
    public float floatingTextOffsetY = -1f;
    public Transform canvasParent;
    public TextMeshProUGUI healthText;// Teks untuk menampilkan HP

    // Variabel Statistik Dasar
    public int maxHealth = 10; 
    public int attackDamage = 1;
    public float hpBarUpdateSpeed = 0.5f;   // Kecepatan Animasi Bar HP
    
    
    [HideInInspector] 
    public int currentHealth;

    // Variabel Internal
    public bool isDead { get; private set; } = false;
    private SpriteRenderer characterRenderer; 
    private Coroutine flashCoroutine; 
    private Coroutine barUpdateCoroutine;   // kontrol Coroutine
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        // Ambil komponen untuk Visual
        characterRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (hpBar != null)
        {
            hpBar.maxValue = maxHealth;
        }
        UpdateHPDisplay();
        
        // Pemicu Animasi IDLE
        if (anim != null)
        {
            anim.SetTrigger("Idle"); 
        }
    }

    // Fungsi untuk memperbarui tampilan HP (teks dan bar)
    private void UpdateHPDisplay()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + "/" + maxHealth;
        }
        if (hpBar != null)
        {
        StartCoroutine(SmoothlyUpdateHPBar(currentHealth));
        }
        barUpdateCoroutine = StartCoroutine(SmoothlyUpdateHPBar(currentHealth));
    }

    // Fungsi untuk menerima damage
    public void TakeDamage(int damage)
    {
        if (isDead) return; // kalau sudah mati, tidak perlu menerima damage lagi
        currentHealth -= damage;    // Kurangi HP
        if (currentHealth <= 0) 
        {
            currentHealth = 0; // Kunci HP ke 0
            isDead = true; 
        }
        Debug.Log(gameObject.name + " menerima " + damage + " damage. Sisa HP: " + currentHealth);
        UpdateHPDisplay();  // Update UI HP ke nilai akhir (0 jika mati)

        // --- VISUAL FEEDBACK: FLOATING DAMAGE TEXT ---
        if (damageTextPrefab != null)
        {
            // Instansiasi di posisi karakter
            Vector3 spawnPos = transform.position + new Vector3(0, floatingTextOffsetY, -10f);
            GameObject floatingText = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity,canvasParent);
            UpdateHPDisplay();
            // Atur angka damage
            DamageText damageScript = floatingText.GetComponent<DamageText>();

            if (damageScript != null)
            {
                damageScript.SetDamageValue(damage);
            }
        }
        
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashEffect(0.15f)); 
        // Jika HP mencapai 0, panggil fungsi Die setelah delay singkat
        if (currentHealth <= 0)
        {   // Tunggu sebentar (sama dengan kecepatan update bar) sebelum memanggil Die()
             // untuk memberikan waktu bagi SmoothlyUpdateHPBar mencapai 0.
            StartCoroutine(ExecuteDeathDelay(hpBarUpdateSpeed));
        }
    }

    IEnumerator ExecuteDeathDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Die();
    }

    private void Die()
    {
        if (!isDead) return; //isDead = true;
        Debug.Log(gameObject.name + " telah dikalahkan!");
        // Pemicu Animasi Kematian
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        else
        {
            gameObject.SetActive(false); 
        }
    }   

    
    // Coroutine untuk efek flashing saat menerima damage
    IEnumerator FlashEffect(float duration)
    {
        if (characterRenderer != null)
        {
            Color originalColor = characterRenderer.color; 
            characterRenderer.color = Color.red; 
            yield return new WaitForSeconds(duration);
            characterRenderer.color = originalColor;
        }
        flashCoroutine = null;
    }   


    // Coroutine untuk memperbarui HP Bar secara halus
    IEnumerator SmoothlyUpdateHPBar(int newHealth)
    {
    float timeElapsed = 0f;
    // Ambil nilai HP Bar saat ini (misalnya 10)
    float oldHealth = hpBar.value; 

    // Loop akan berjalan sampai waktu yang ditentukan selesai
    while (timeElapsed < hpBarUpdateSpeed)
    {   // Hitung nilai baru di antara HP lama (oldHealth) dan HP baru (newHealth)
        hpBar.value = Mathf.Lerp(oldHealth, newHealth, timeElapsed / hpBarUpdateSpeed);
        timeElapsed += Time.deltaTime;  // Tambahkan waktu yang sudah berlalu (waktu antar frame)
        // Tunggu frame berikutnya
        yield return null; 
    }
    // Pastikan bar berhenti tepat di nilai HP baru, memastikan ia mencapai 0 jika newHealth adalah 0
    hpBar.value = newHealth;
    barUpdateCoroutine = null; // Tandai Coroutine selesai
    }
}