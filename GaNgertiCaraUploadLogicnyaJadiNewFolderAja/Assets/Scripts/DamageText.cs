using UnityEngine;
using TMPro; // Wajib ada untuk menggunakan TextMeshPro
using System.Collections;

public class DamageText : MonoBehaviour
{
    private TextMeshPro textMesh;

    // Variabel yang bisa disesuaikan di Inspector Prefab:
    public float moveSpeed = 1.5f; // Kecepatan teks melayang ke atas
    public float duration = 1.0f;  // Durasi teks ditampilkan sebelum hilang
    
    private Vector3 moveDirection;

    void Awake()
    {
        // 1. Ambil komponen TextMeshPro
        textMesh = GetComponent<TextMeshPro>();
        
        // 2. Tentukan arah melayang (ke atas dan sedikit ke kanan)
        moveDirection = new Vector3(0.5f, 1f, 0f).normalized;
        
        // 3. Mulai Coroutine untuk menghancurkan objek setelah durasi
        StartCoroutine(DestroyAfterTime());
    }

    void Update()
    {
        // Gerakkan teks ke arah moveDirection
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Perlahan-lahan buat teks memudar (menjadi transparan)
        if (textMesh != null)
        {
            textMesh.color = Color.Lerp(textMesh.color, Color.clear, Time.deltaTime / duration);
        }
    }

    // Fungsi Publik untuk dipanggil oleh CharacterStats.cs
    public void SetDamageValue(int damage)
    {
        if (textMesh != null)
        {
            // Tampilkan angka damage
            textMesh.text = damage.ToString();
        }
    }
 
    IEnumerator DestroyAfterTime()
    {
        // Tunggu sesuai durasi (misal, 1 detik)
        yield return new WaitForSeconds(duration);
        
        // Hancurkan objek Floating Text
        Destroy(gameObject); 
    }
}