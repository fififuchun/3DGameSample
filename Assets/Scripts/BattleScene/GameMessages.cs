using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mergepins
{
    // プレイヤーがどの行動を選択したか
    public enum PlayerAction
    {
        None = 0,
        Shield = 1,
        Merge = 2,
        Switch = 3,
        Finisher = 4,
        Attacks = 5,
        ElemActions = 6
    }

    // ゲームの進行状況
    public enum GameState
    {
        Initial,
        Wait,
        Judge,
        MyTurn, EnemyTurn,
        MyTurnChoose, EnemyTurnChoose,
        MySwitch, EnemySwitch,
        TurnEnd,
        MyEnd, EnemyEnd,
        MyEndChoose, EnemyEndChoose,
        Complete
    }

    [Flags]
    public enum PlayerField
    {
        None = 0,
        Guard = 1 << 0,
        Merge = 1 << 1,
        NullifyDef = 1 << 2,
        Wadatsumi = 1 << 3,
        Undergrowth = 1 << 4,
        Kusanagi = 1 << 5
    }

    public static class EnumExtension
    {
        /// <summary>
        /// 同じPlayerFieldを持っているか判定する関数
        /// </summary>
        /// <param name="value">現在のPlayerField</param>
        /// <param name="field">判定したいPlayerField</param>
        /// <returns>持っているならTrue,いないならFalse</returns>
        public static bool HasField(this PlayerField value, PlayerField field) => (value & field) == field;

        /// <summary>
        /// PlayerFieldの状態をboolの配列にして返す関数
        /// </summary>
        /// <returns>boolの配列</returns>
        public static bool[] ToBoolArray(this PlayerField value)
        {
            List<bool> bool_list = new List<bool>();
            foreach (PlayerField field in Enum.GetValues(typeof(PlayerField)))
                bool_list.Add(value.HasField(field));
            bool_list.RemoveAt(0);
            return bool_list.ToArray();
        }
    }

    public static class AnimatorUtilities
    {
        public static IEnumerator WaitForCurrentAnimation(this Animator animator, int layer_number = 0)
        {
            yield return null;
            int state_hash = animator.GetCurrentAnimatorStateInfo(layer_number).fullPathHash;
            yield return new WaitWhile(() =>
            {
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(layer_number);
                return current.normalizedTime < 1 && current.fullPathHash == state_hash;
            });
        }

        public static IEnumerator AnimationCoroutine(this Animator animator, string playStateName, string backStateName)
        {
            animator.CrossFade(playStateName, 0);
            yield return animator.WaitForCurrentAnimation();
            animator.CrossFade(backStateName, 0);
        }
    }

    public static class CoroutineUtilities
    {
        public static IEnumerator WaitForAllCoroutines(Coroutine[] coroutines)
        {
            foreach (Coroutine coroutine in coroutines)
                yield return coroutine;
        }
    }
}
