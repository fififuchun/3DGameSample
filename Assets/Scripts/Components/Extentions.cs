using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 拡張メソッド
namespace Mergepins
{
    public static class Extentions
    {
        // キャンバスグループの有効化。引数は0から1まで(float)の透明度を表す数値。
        public static void Enable(this CanvasGroup canvasGroup, float alpha)
        {
            DOTween.To(() => canvasGroup.alpha, (x) => canvasGroup.alpha = x, alpha, 0.24f).SetEase(Ease.OutCubic).SetLink(canvasGroup.gameObject);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // キャンバスグループの有効化。alphaを指定しない(変えない)場合。
        public static void Enable(this CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // キャンバスグループの無効化。引数は0から1まで(float)の透明度を表す数値。
        public static void Disable(this CanvasGroup canvasGroup, float alpha)
        {
            DOTween.To(() => canvasGroup.alpha, (x) => canvasGroup.alpha = x, alpha, 0.24f).SetEase(Ease.OutCubic).SetLink(canvasGroup.gameObject);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // キャンバスグループの無効化。alphaを指定しない(変えない)場合。
        public static void Disable(this CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
