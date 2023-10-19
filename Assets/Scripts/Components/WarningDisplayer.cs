using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class WarningDisplayer : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField]
    private TextMeshProUGUI text;
    private Sequence sequence;

    private void Awake()
    {
        this.canvasGroup = GetComponent<CanvasGroup>();
    }

    public void DisplayWarning(string message)
    {
        if (sequence != null && sequence.IsActive() && sequence.IsPlaying()) sequence.Kill();
        text.text = message;
        sequence = DOTween.Sequence()
        .Append(DOTween.To(() => canvasGroup.alpha, (x) => canvasGroup.alpha = x, 1, 0.4f).SetEase(Ease.Linear).SetLink(canvasGroup.gameObject))
        .AppendInterval(0.3f)
        .Append(DOTween.To(() => canvasGroup.alpha, (x) => canvasGroup.alpha = x, 0, 0.4f).SetEase(Ease.Linear).SetLink(canvasGroup.gameObject));
    }
}
