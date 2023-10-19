using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using Mergepins;

public class DragPin : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private CanvasGroup backCanvasGroup;
    [SerializeField]
    private CanvasGroup buttonCanvasGroup;
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
    private PinLayout pinLayout;

    private void Awake()
    {
        this.parent = this.transform.parent;
        this.root = this.parent.parent.parent;
        this.pinLayout = this.parent.GetComponent<PinLayout>();
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
            this.transform.SetSiblingIndex(1);
            backCanvasGroup.Disable(0.0f);
            buttonCanvasGroup.Disable();
            trashCanvasGroup.Enable(1.0f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (this.dragging)
        {
            this.transform.position = eventData.position;
            this.Replace();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.dragging)
        {
            this.dragging = false;
            this.pointer = eventData;
            backCanvasGroup.Enable(1.0f);
            buttonCanvasGroup.Enable();
            trashCanvasGroup.Disable(0.0f);
            this.placeholder.transform.SetParent(this.root);
            Destroy(this.placeholder);
            if (this.Trash)
            {
                this.selection.RemovePin(this.beforeIndex);
                Destroy(gameObject);
                this.pinLayout.Align(this.afterIndex);
                this.Trash = false;
                return;
            }
            this.Replace();
            this.selection.ReplacePins(this.beforeIndex, this.afterIndex);
        }
    }

    public void CreatePlaceHolder()
    {
        this.placeholder = new GameObject();
        this.placeholder.transform.SetParent(this.parent, false);
        this.placeholder.transform.SetSiblingIndex(this.beforeIndex);
        Image image = this.placeholder.AddComponent<Image>();
        image.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        RectTransform rectTransform = this.placeholder.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(140f, 180f);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        this.pinLayout.SetPosition(this.placeholder.transform, this.beforeIndex);
    }

    public void Replace()
    {
        int index = this.afterIndex;
        if (this.afterIndex > 0)
        {
            if (this.transform.position.x < this.parent.GetChild(this.afterIndex - 1).position.x)
            {
                index--;
            }
        }
        if (this.afterIndex < this.parent.childCount - 1)
        {
            if (this.transform.position.x > this.parent.GetChild(this.afterIndex + 1).position.x)
            {
                index++;
            }
        }

        if (this.dragging)
        {
            if (index != this.afterIndex)
            {
                this.placeholder.transform.SetSiblingIndex(index);
                this.pinLayout.SetPosition(this.placeholder.transform, index);
                this.pinLayout.SwitchMovement(this.parent.GetChild(this.afterIndex), this.afterIndex);
            }
        }
        else
        {
            this.transform.SetParent(this.parent, false);
            this.transform.SetSiblingIndex(index);
            this.transform.position = this.pointer.position;
            this.pinLayout.DropMovement(this.transform, index);
        }
        this.afterIndex = index;
    }
}
