using UnityEngine;
using DG.Tweening;

public class TeamLayout : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField]
    private float posx;
    [SerializeField]
    private float height;
    [SerializeField]
    private float top;
    [SerializeField]
    private float bottom;
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
        this.FitSize();
    }

    public void SetPosition(Transform transform, int index)
    {
        RectTransform rect = (RectTransform)transform;
        float y = this.top + this.height * index + this.spacing * index + this.height / 2;
        rect.anchoredPosition = new Vector2(this.posx, -y);
    }

    public void FitSize()
    {
        int count = this.transform.childCount;
        float y = this.top + this.height * count + this.spacing * (count - 1) + this.bottom;
        this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, y);
    }

    public void SwitchMovement(Transform transform, int index)
    {
        RectTransform rect = (RectTransform)transform;
        if (DOTween.IsTweening(rect))
        {
            rect.DOKill();
        }
        float y = this.top + this.height * index + this.spacing * index + this.height / 2;
        rect.DOAnchorPosY(-y, 0.28f).SetEase(Ease.OutCubic);
    }

    public void DropMovement(Transform transform, int index)
    {
        RectTransform rect = (RectTransform)transform;
        float y = this.top + this.height * index + this.spacing * index + this.height / 2;
        rect.DOAnchorPos(new Vector2(this.posx, -y), 0.24f).SetEase(Ease.OutCubic);
    }
}