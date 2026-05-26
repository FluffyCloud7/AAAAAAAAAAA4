using UnityEngine;
using UnityEngine.UI;

public class BirdFollowMouse : MonoBehaviour
{
    public static BirdFollowMouse Instance;

    [Header("Движение за мышью")]
    public float followSpeed = 15f;

    [Header("Смещение относительно курсора")]
    public Vector3 cursorOffset = new Vector3(60f, -20f, 0f); // X = 60 (правее), Y = -20 (чуть ниже курсора)

    [Header("Покачивание в полете")]
    public float waveSpeed = 5f;
    public float waveHeight = 15f;

    private Image _image;
    private RectTransform _rectTransform;
    private bool _isActive = false;

    private void Awake()
    {
        Instance = this;
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        HideBird();
    }

    public void ShowBird()
    {
        _isActive = true;
        if (_image != null) _image.enabled = true;

        foreach (var img in GetComponentsInChildren<Image>())
        {
            img.enabled = true;
        }

        // При появлении сразу учитываем смещение
        _rectTransform.position = Input.mousePosition + cursorOffset;
    }

    public void HideBird()
    {
        _isActive = false;
        if (_image != null) _image.enabled = false;

        foreach (var img in GetComponentsInChildren<Image>())
        {
            img.enabled = false;
        }
    }

    void Update()
    {
        if (!_isActive) return;

        // 1. Берем позицию мыши и добавляем к ней наше смещение вбок
        Vector3 targetPosition = Input.mousePosition + cursorOffset;

        // 2. Считаем покачивание по синусоиде вверх-вниз
        float wobble = Mathf.Sin(Time.time * waveSpeed) * waveHeight;
        targetPosition.y += wobble;

        // 3. Плавно ведем птицу в эту смещенную точку
        _rectTransform.position = Vector3.Lerp(_rectTransform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}