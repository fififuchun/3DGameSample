using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Mergepins;

public class DragTeam : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private RectTransform rect;
    [SerializeField]
    private Scrollbar scrollbar;
    private float bottomBorder;
    private float topBorder;
    [SerializeField]
    private CanvasGroup backCanvasGroup;
    [SerializeField]
    private CanvasGroup rebuildCanvasGroup;
    [SerializeField]
    private CanvasGroup trashCanvasGroup;
    [SerializeField]
    private Selection selection;
    private Transform parent;
    private Transform root;
    private GameObject placeholder;
    private int beforeIndex;
    private int afterIndex;
    private bool dragging = false;
    private PointerEventData pointer;
    public bool Trash { get; set; } = false;
    public bool Rebuild { get; set; } = false;
    private TeamLayout teamLayout;

    private void Awake()
    {
        this.bottomBorder = this.rect.sizeDelta.y * this.rect.localScale.y * 0.1f;
        this.topBorder = this.rect.sizeDelta.y * this.rect.localScale.y * 0.9f;
        this.parent = this.transform.parent;
        this.root = this.parent.parent.parent.parent;
        this.teamLayout = this.parent.GetComponent<TeamLayout>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!this.dragging)
        {
            this.dragging = true;
            this.beforeIndex = this.transform.GetSiblingIndex();
            this.afterIndex = this.beforeIndex;
            this.CreatePlaceHolder();
            this.transform.DOKill();
            this.transform.SetParent(this.root);
            this.transform.SetSiblingIndex(2);
            this.backCanvasGroup.Disable(0.0f);
            this.rebuildCanvasGroup.Enable(1.0f);
            this.trashCanvasGroup.Enable(1.0f);
            this.selection.scrollContentCanvasGroup.Disable();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.dragging)
        {
            this.transform.position = eventData.position;
            this.Replace();
            this.pointer = eventData;
        }
    }

    private void Update()
    {
        if (this.dragging)
        {
            if (this.bottomBorder > this.pointer.position.y && this.scrollbar.value > 0.0f)
            {
                this.scrollbar.value -= (this.bottomBorder - this.pointer.position.y) * 0.0001f;
                this.Replace();
            }
            if (this.topBorder < this.pointer.position.y && this.scrollbar.value < 1.0f)
            {
                this.scrollbar.value += (this.pointer.position.y - this.topBorder) * 0.0001f;
                this.Replace();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.dragging)
        {
            this.dragging = false;
            this.backCanvasGroup.Enable(1.0f);
            this.rebuildCanvasGroup.Disable(0.0f);
            this.trashCanvasGroup.Disable(0.0f);
            this.selection.scrollContentCanvasGroup.Enable();
            this.placeholder.transform.SetParent(this.root);
            Destroy(this.placeholder);
            if (this.Trash)
            {
                this.selection.RemoveTeam(this.beforeIndex);
                Destroy(gameObject);
                this.teamLayout.Align(this.afterIndex);
                this.Trash = false;
                return;
            }
            this.Replace();
            this.selection.ReplaceTeams(this.beforeIndex, this.afterIndex);
            if (this.Rebuild)
            {
                this.selection.RebuildTeamButton(this.afterIndex);
                this.Rebuild = false;
            }
        }
    }

    public void CreatePlaceHolder()
    {
        this.placeholder = new GameObject();
        this.placeholder.transform.SetParent(this.parent, false);
        this.placeholder.transform.SetSiblingIndex(this.beforeIndex);
        Image image = this.placeholder.AddComponent<Image>();
        image.color = new Color(0.0f, 0.0f, 0.0f, 0f / 255f);
        RectTransform rectTransform = this.placeholder.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(960f, 355f);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        this.teamLayout.SetPosition(this.placeholder.transform, this.beforeIndex);
    }

    public void Replace()
    {
        int index = this.afterIndex;
        if (this.afterIndex > 0)
        {
            if (this.transform.position.y > this.parent.GetChild(this.afterIndex - 1).position.y)
            {
                index--;
            }
        }
        if (this.afterIndex < this.parent.childCount - 1)
        {
            if (this.transform.position.y < this.parent.GetChild(this.afterIndex + 1).position.y)
            {
                index++;
            }
        }

        if (this.dragging)
        {
            if (index != this.afterIndex)
            {
                this.placeholder.transform.SetSiblingIndex(index);
                this.teamLayout.SetPosition(this.placeholder.transform, index);
                this.teamLayout.SwitchMovement(this.parent.GetChild(this.afterIndex), this.afterIndex);
            }
        }
        else
        {
            this.transform.SetParent(this.parent, false);
            this.transform.SetSiblingIndex(index);
            this.transform.position = this.pointer.position;
            this.teamLayout.DropMovement(this.transform, index);
        }
        this.afterIndex = index;
    }
}
