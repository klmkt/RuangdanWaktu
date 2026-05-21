using UnityEngine;
using UnityEngine.EventSystems; // Wajib ditambahkan untuk mendeteksi kursor
using TMPro; // Wajib ditambahkan untuk mengakses TextMeshPro

// Menggunakan interface IPointerEnterHandler dan IPointerExitHandler
public class ButtonTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _buttonText;

    [Header("Color Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _hoverColor = Color.blue;

    private void Start()
    {
        // Set warna default saat game mulai
        if (_buttonText != null)
        {
            _buttonText.color = _normalColor;
        }
        else
        {
            Debug.LogWarning("Text TMP belum dimasukkan ke script ButtonTextHover di: " + gameObject.name);
        }
    }

    // Fungsi ini otomatis terpanggil saat kursor masuk ke area Button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_buttonText != null)
        {
            _buttonText.color = _hoverColor;
        }
    }

    // Fungsi ini otomatis terpanggil saat kursor keluar dari area Button
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_buttonText != null)
        {
            _buttonText.color = _normalColor;
        }
    }
}