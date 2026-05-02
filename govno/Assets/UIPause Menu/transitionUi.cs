using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonVisual : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public Image targetImage;

    public Sprite idleSprite;
    public Sprite hoverSprite;

    public Color normalColor = Color.white;
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f); // затемнение

    private bool isHovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetImage.sprite = idleSprite;
        targetImage.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetImage.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetImage.color = normalColor;

        if (isHovering)
            targetImage.sprite = hoverSprite;
        else
            targetImage.sprite = idleSprite;
    }
}