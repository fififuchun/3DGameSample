using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapScrollElement : MonoBehaviour
{
    private int index;
    private RectTransform rect;
    [SerializeField]
    private ScrollRectSnap snap;
    [SerializeField]
    private float r;

    private void Awake()
    {
        this.index = this.transform.GetSiblingIndex();
        this.rect = (RectTransform)this.transform;
    }

    private void Update()
    {
        float distance = Mathf.Abs(this.snap.content.anchoredPosition.y - this.snap.distance * this.index) / this.snap.distance;
        if (distance <= 1)
            this.transform.localScale = Vector3.one * (1f + 0.2f * Mathf.Pow(1 - distance, 2));
    }
}
