using UnityEngine;
using UnityEngine.EventSystems; // Untuk mendeteksi kursor
using UnityEngine.UI; // Wajib ditambahkan untuk mengakses komponen Image

public class ButtonSpriteHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _buttonImage;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _hoverSprite;

    private void Start()
    {
        // Pastikan sprite kembali ke normal saat game dimulai
        if (_buttonImage != null && _normalSprite != null)
        {
            _buttonImage.sprite = _normalSprite;
        }
        else
        {
            Debug.LogWarning("Komponen Image atau Normal Sprite belum dimasukkan di: " + gameObject.name);
        }
    }

    // Terpanggil saat kursor masuk area button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_buttonImage != null && _hoverSprite != null)
        {
            _buttonImage.sprite = _hoverSprite;
        }
    }

    // Terpanggil saat kursor keluar area button
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_buttonImage != null && _normalSprite != null)
        {
            _buttonImage.sprite = _normalSprite;
        }
    }
}