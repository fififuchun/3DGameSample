using UnityEngine;
using DG.Tweening;

public class PinLayout : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField]
    private float posy;
    [SerializeField]
    private float width;
    [SerializeField]
    private float left;
    [SerializeField]
    private float spacing;

    private void Awake()
    {
        this.rectTransform = (RectTransform)this.transform;
    }

    public void Align(int index)
    {
        for (int i = index; i < this.transform.childCount; i++)
        {
            this.SwitchMovement(this.transform.GetChild(i), i);
        }
    }

    public void SetPosition(Transform transform, int index)
    {
        RectTransform rect = (RectTransform)transform;
        float x = this.left + this.width * index + this.spacing * index + this.width / 2;
        rect.anchoredPosition = new Vector2(x, this.posy);
    }

    public void SwitchMovement(Transform transform, int index)
    {
        RectTransform rect = (RectTransform)transform;
        if (DOTween.IsTweening(rect))
        {
            rect.DOKill();
        }
        float x = this.left + this.width * index + this.spacing * index + this.width / 2;
        rect.DOAnchorPosX(x, 0.28f).SetEase(Ease.OutCubic);
    }

    public void DropMovement(Transform transform, int index)
    {
        RectTransform rect = (RectTransform)transform;
        float x = this.left + this.width * index + this.spacing * index + this.width / 2;
        rect.DOAnchorPos(new Vector2(x, this.posy), 0.24f).SetEase(Ease.OutCubic);
    }
}