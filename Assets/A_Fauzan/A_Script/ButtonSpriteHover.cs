using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// [RequireComponent] memastikan script ini HANYA bisa dipasang di objek yang punya komponen Image
[RequireComponent(typeof(Image))]
public class ButtonSpriteHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _buttonImage;
    private Sprite _normalSprite; // Sekarang private, tidak perlu diisi di Inspector

    [Header("Hover Settings")]
    [SerializeField] private Sprite _hoverSprite; // Kamu HANYA perlu mengisi ini di Inspector

    private void Awake()
    {
        // Secara otomatis mengambil komponen Image yang ada di GameObject tombol ini
        _buttonImage = GetComponent<Image>();

        // Membaca 'Source Image' yang sudah terpasang dan menyimpannya sebagai Normal Sprite
        if (_buttonImage != null)
        {
            _normalSprite = _buttonImage.sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_buttonImage != null && _hoverSprite != null)
        {
            _buttonImage.sprite = _hoverSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_buttonImage != null && _normalSprite != null)
        {
            // Mengembalikan ke Source Image aslinya
            _buttonImage.sprite = _normalSprite;
        }
    }
}