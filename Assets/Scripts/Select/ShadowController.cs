using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ShadowController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject shadow;
    private RectTransform rect;
    private RectTransform shadowRect;
    private Vector2 shadowPos = new Vector2();

    private void Awake()
    {
        this.rect = (RectTransform)this.transform;
        this.shadowRect = (RectTransform)this.shadow.transform;
        this.shadowPos = (this.shadowRect).anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        this.ShrinkShadow();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.ExpandShadow();
    }

    private void ShrinkShadow()
    {
        this.shadow.transform.DOScale(0.94f, 0.24f).SetEase(Ease.OutCubic).SetLink(this.shadow);
        this.shadowRect.DOAnchorPos(this.rect.anchoredPosition, 0.24f).SetEase(Ease.OutCubic).SetLink(this.shadow);
    }

    private void ExpandShadow()
    {
        this.shadow.transform.DOScale(1.0f, 0.24f).SetEase(Ease.OutCubic).SetLink(this.shadow);
        this.shadowRect.DOAnchorPos(this.shadowPos, 0.24f).SetEase(Ease.OutCubic).SetLink(this.shadow);
    }
}
