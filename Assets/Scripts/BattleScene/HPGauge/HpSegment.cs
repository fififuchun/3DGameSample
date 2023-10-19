using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mergepins;

public class HpSegment : MonoBehaviour
{
    [SerializeField] private TwoDigitSegment sevenSegmentHP;

    private int maxHP;
    private int referenceHP;
    private float currentHP;

    public int MaxHP { get { return maxHP; } set { maxHP = value; UpdateAll(); } }
    public float CurrentHP { get { return currentHP; } set { currentHP = value; UpdateAll(); } }

    private Gradient gradient = new Gradient();

    private void Awake()
    {
        GradientColorKey[] gradientColorKeys = new GradientColorKey[4];
        gradientColorKeys[0].color = HpGauge.color0;
        gradientColorKeys[0].time = HpGauge.normalized_points[0];
        gradientColorKeys[1].color = HpGauge.color1_4;
        gradientColorKeys[1].time = HpGauge.normalized_points[1];
        gradientColorKeys[2].color = HpGauge.color1_2;
        gradientColorKeys[2].time = HpGauge.normalized_points[2];
        gradientColorKeys[3].color = HpGauge.color1;
        gradientColorKeys[3].time = HpGauge.normalized_points[3];
        gradient.SetKeys(gradientColorKeys, new GradientAlphaKey[0]);
    }

    public IEnumerator HPTween(int referenceHP)
    {
        this.referenceHP = referenceHP;
        yield return DOTween.To(() => CurrentHP, (x) => CurrentHP = x, this.referenceHP, 3.0f).SetEase(Ease.InOutCubic).WaitForCompletion();
    }

    private void UpdateAll()
    {
        SetColors(CurrentHP / maxHP);
        SetHPNumber(CurrentHP);
    }

    private void SetColors(float value)
    {
        float normalized = Mathf.Clamp(value, 0.0f, 1.0f);
        Color color = gradient.Evaluate(normalized);
        sevenSegmentHP.SetColor(color);
    }

    private void SetHPNumber(float value)
    {
        int floored = Mathf.CeilToInt(value < 0 ? 0 : value);
        sevenSegmentHP.DisplayNumber(floored);
    }
}
