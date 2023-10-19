using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Mergepins;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager instance;
    private void Awake() { instance = this; }

    public IEnumerator AttackAnimation(Player self, Player opponent)
    {
        Coroutine[] coroutines = new Coroutine[]{StartCoroutine(PinAttackTween(self.topObject.transform))
                                                // ,StartCoroutine(animator.AnimationCoroutine("Play", "Idle"))
                                                };
        yield return CoroutineUtilities.WaitForAllCoroutines(coroutines);

        Coroutine[] coroutines1 = new Coroutine[7];
        coroutines1[0] = StartCoroutine(opponent.topHpGauge.HPTween(opponent.hpList[opponent.topIndex]));
        for (int i = 0; i < 6; i++)
            coroutines1[i + 1] = StartCoroutine(opponent.hpSegments[i].HPTween(opponent.hpList[i]));
        yield return CoroutineUtilities.WaitForAllCoroutines(coroutines1);
    }

    private IEnumerator PinAttackTween(Transform transform)
    {
        yield return DOTween.To(() => transform.position, x => transform.position = x, new Vector3(1, 1, 1), 2.0f).SetRelative().WaitForCompletion();
        yield return DOTween.To(() => transform.position, x => transform.position = x, -new Vector3(1, 1, 1), 2.0f).SetRelative().WaitForCompletion();
    }
}
