using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ScrollRectSnap : ScrollRect
{
    [SerializeField]
    public float posx;
    [SerializeField]
    private float height;
    [SerializeField]
    private float view_height;
    [SerializeField]
    private float spacing;
    [SerializeField]
    private float minVelocity;
    public float distance { get; private set; }
    private bool can_set_index;
    public int index { get; private set; }
    private int childCount;

    public event Action BeginSetIndex;
    public event Action CompleteSetIndex;

    protected override void Awake()
    {
        this.distance = this.height + this.spacing;
        this.can_set_index = false;
        this.index = 0;
    }

    private void Update()
    {
        if (this.can_set_index && Mathf.Abs(this.velocity.y) < this.minVelocity)
        {
            this.index = Mathf.RoundToInt(this.content.anchoredPosition.y / this.distance);
            if (this.index < 0)
                this.index = 0;
            if (this.index > this.childCount - 1)
                this.index = this.childCount - 1;
            this.content.DOAnchorPosY(this.distance * this.index, 0.26f).SetEase(Ease.OutQuad).OnComplete(() => this.CompleteSetIndex?.Invoke());
            this.can_set_index = false;
        }
    }

    public override void OnScroll(PointerEventData data)
    {
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        this.can_set_index = false;
        this.content.DOKill();
        base.OnBeginDrag(eventData);
        this.BeginSetIndex?.Invoke();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        this.can_set_index = true;
        base.OnEndDrag(eventData);
    }

    public void OneSnapUp()
    {
        if (this.index > 0)
        {
            this.index--;
        }
        this.content.DOKill();
        this.BeginSetIndex?.Invoke();
        this.content.DOAnchorPosY(this.distance * this.index, 0.26f).SetEase(Ease.OutQuad).OnComplete(() => this.CompleteSetIndex?.Invoke());
    }

    public void OneSnapDown()
    {
        if (this.index < this.childCount - 1)
        {
            this.index++;
        }
        this.content.DOKill();
        this.BeginSetIndex?.Invoke();
        this.content.DOAnchorPosY(this.distance * this.index, 0.26f).SetEase(Ease.OutQuad).OnComplete(() => this.CompleteSetIndex?.Invoke());
    }

    public void Init()
    {
        this.can_set_index = false;
        this.index = 0;
        this.content.anchoredPosition = new Vector2(this.content.anchoredPosition.x, 0);
        this.childCount = this.content.childCount;
        for (int i = 0; i < this.childCount; i++)
        {
            this.SetPosition(this.content.GetChild(i), i);
        }
        this.FitSize();
    }

    public void SetPosition(Transform transform, int indx)
    {
        RectTransform rect = (RectTransform)transform;
        float y = this.view_height / 2 + this.distance * indx;
        rect.anchoredPosition = new Vector2(this.posx, -y);
    }

    public void FitSize()
    {
        float y = this.view_height + this.distance * (this.content.childCount - 1);
        this.content.sizeDelta = new Vector2(this.content.sizeDelta.x, y);
    }
}
