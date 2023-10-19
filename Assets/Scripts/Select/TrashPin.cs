using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashPin : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image trashImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.trashImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.trashImage.color = new Color(0.0f, 0.0f, 0.0f, 130f / 255f);
    }

    public void OnDrop(PointerEventData eventData)
    {
        DragPin pin = eventData.pointerDrag.GetComponent<DragPin>();
        if (pin != null)
        {
            pin.Trash = true;
        }
    }
}
