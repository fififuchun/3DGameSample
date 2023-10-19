using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;

public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField]
    private Image image;
    public void SetImage(Image image)
    {
        this.image = image;
        this.normalColor = image.color;
    }
    public Color pressedColor;
    private Color normalColor;
    public UnityEvent OnClick = new UnityEvent();

    private void Awake()
    {
        if (this.image != null)
            this.normalColor = this.image.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.image.transform.DOScale(0.94f, 0.24f).SetEase(Ease.OutCubic).SetLink(this.gameObject);
        this.image.DOColor(this.pressedColor, 0.24f).SetEase(Ease.OutCubic).SetLink(this.gameObject);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.image.transform.DOScale(1f, 0.24f).SetEase(Ease.OutCubic).SetLink(this.gameObject);
        this.image.DOColor(this.normalColor, 0.24f).SetEase(Ease.OutCubic).SetLink(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.OnClick.Invoke();
    }
}
