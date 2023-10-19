using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Mergepins;

public class HpGauge : MonoBehaviour
{
    [SerializeField] private Image hpFill;
    [SerializeField] private TwoDigitSegment sevenSegmentHPMax;
    [SerializeField] private PerSegment twoSegmentBar;
    [SerializeField] private TwoDigitSegment sevenSegmentHP;
    [SerializeField] private RectTransform pointer;

    private int maxHP;
    private int referenceHP;
    private float currentHP;

    public int MaxHP { get { return maxHP; } set { maxHP = value; sevenSegmentHPMax.DisplayNumber(maxHP); UpdateAll(); } }
    public float CurrentHP { get { return currentHP; } set { currentHP = value; UpdateAll(); } }

    private static readonly Vector2 circle_center_pos = new Vector2(0, 0);
    private static readonly float radius = 413.1965f;

    private static readonly float angle_offset = -47.4628f;

    private static readonly float[] angle_points = new float[4] { -144f, -89.82f, -34.128f, 90f };
    private static readonly float[] fill_amount_points = new float[4] { 0.35f, 0.5005f, 0.6552f, 1.0f };
    public static readonly float[] normalized_points = new float[4] { 0.0f, 1.0f / 4.0f, 1.0f / 2.0f, 1.0f };

    private Gradient gradient = new Gradient();
    public static readonly Color color1 = new Color(0, 204f / 255f, 37f / 255f, 1);
    public static readonly Color color1_2 = new Color(1, 254f / 255f, 16f / 255f, 1);
    public static readonly Color color1_4 = new Color(1, 16f / 255f, 16f / 255f, 1);
    public static readonly Color color0 = new Color(0, 0, 0, 1);

    private void Awake()
    {
        GradientColorKey[] gradientColorKeys = new GradientColorKey[4];
        gradientColorKeys[0].color = color0;
        gradientColorKeys[0].time = normalized_points[0];
        gradientColorKeys[1].color = color1_4;
        gradientColorKeys[1].time = normalized_points[1];
        gradientColorKeys[2].color = color1_2;
        gradientColorKeys[2].time = normalized_points[2];
        gradientColorKeys[3].color = color1;
        gradientColorKeys[3].time = normalized_points[3];
        gradient.SetKeys(gradientColorKeys, new GradientAlphaKey[0]);

        twoSegmentBar.Display(true);
    }

    public IEnumerator HPTween(int referenceHP)
    {
        this.referenceHP = referenceHP;
        yield return DOTween.To(() => CurrentHP, (x) => CurrentHP = x, this.referenceHP, 3.0f).SetEase(Ease.InOutCubic).WaitForCompletion();
    }

    private void UpdateAll()
    {
        SetPointerTransform(CurrentHP / maxHP);
        SetFillAmount(CurrentHP / maxHP);
        SetColors(CurrentHP / maxHP);
        SetHPNumber(CurrentHP);
    }

    private void SetPointerTransform(float value)
    {
        float normalized = Mathf.Clamp(value, 0.0f, 1.0f);
        float angle = 0;
        for (int i = 1; i < 4; i++)
        {
            if (normalized <= normalized_points[i])
            {
                angle = Mathf.Lerp(angle_points[i - 1], angle_points[i], (normalized - normalized_points[i - 1]) / (normalized_points[i] - normalized_points[i - 1]));
                break;
            }
        }
        float radian = angle * Mathf.Deg2Rad;
        pointer.anchoredPosition = circle_center_pos + radius * new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        pointer.rotation = Quaternion.Euler(0, 0, -angle_offset + angle + 180);
    }

    private void SetFillAmount(float value)
    {
        float normalized = Mathf.Clamp(value, 0.0f, 1.0f);
        float fill_amount = 0;
        for (int i = 1; i < 4; i++)
        {
            if (normalized <= normalized_points[i])
            {
                fill_amount = Mathf.Lerp(fill_amount_points[i - 1], fill_amount_points[i], (normalized - normalized_points[i - 1]) / (normalized_points[i] - normalized_points[i - 1]));
                break;
            }
        }
        hpFill.fillAmount = fill_amount;
    }

    private void SetColors(float value)
    {
        float normalized = Mathf.Clamp(value, 0.0f, 1.0f);
        Color color = gradient.Evaluate(normalized);
        hpFill.color = color;
        twoSegmentBar.SetColor(color);
        sevenSegmentHP.SetColor(color);
        sevenSegmentHPMax.SetColor(color);
    }

    private void SetHPNumber(float value)
    {
        int floored = Mathf.CeilToInt(value < 0 ? 0 : value);
        sevenSegmentHP.DisplayNumber(floored);
    }
}
