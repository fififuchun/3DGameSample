using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mergepins;
using Mergepins.Network;

public class Player : MonoBehaviour
{
    #region Status

    public PlayerAction playerAction { get; private set; }
    public PlayerField playerField = PlayerField.None;

    public void SetPlayerAction(PlayerAction _action)
    {
        playerAction = _action;
        Debug.Log("Player State was setted as " + _action);
    }

    public void InformedPlayerActions(MatchPlayerAction action)
    {
        playerAction = action.playerAction;
        wood_action = action.woodAction;
        if (action.newly_merged_pins != null)
            newly_merged_pins = new HashSet<int>(action.newly_merged_pins);
        change_index = action.change_index;
        storm_elem_action_index = action.storm_elem_action_index;
        norm_action_type = action.norm_action_type;
        regenerate_selected_index = action.regenerate_selected_index;
    }

    public void AddField(PlayerField added_field)
    {
        playerField |= added_field;
        UIManager.instance.OnPlayerFieldAdded();
    }
    public void RemoveField(PlayerField removed_field)
    {
        playerField &= ~removed_field;
        UIManager.instance.OnPlayerFieldRemoved();
    }

    //ゲーム内で使うステータスに関する変数、HpはUpdateでスライダーに反映するために監視
    public int[] hpList = new int[6];
    public int[] attackList = new int[6];

    //ステータスコーナー、pinsMaxHp[0]はblazeのHpで他も同様、ここの値はゲーム全体を通して基本固定
    private int[] pinsMaxHp = new int[10] { 4, 4, 8, 4, 4, 4, 4, 4, 8, 4 };
    private int[] pinsAttack = new int[10] { 4, 2, 2, 1, 2, 2, 2, 2, 2, 2 };

    //パーティーメンバー用の配列、この数字をいじるとパーティーメンバーが変わります
    public int[] party;

    //パーティーの最初に出るピンのパーティの中での順番
    public int topIndex;

    /// <summary>
    /// パーティーのバトル場に出ているピンのタイプを返す
    /// </summary>
    /// <returns>int</returns>
    public int GetTopType() => party[topIndex];
    /// <summary>
    /// パーティーのindex番目のピンのタイプを返す
    /// </summary>
    /// <returns>indexが0から5の間じゃないときは-1を返す</returns>
    public int GetTypeByIndex(int index) => (-1 < index && index < 6) ? party[index] : -1;

    /// <summary> ベースとなるピンに対して、partyの何番目と何番目がマージされているのかを表す。 </summary>
    public HashSet<int> merged_pins = new HashSet<int>();

    /// <summary> partyの何番目(index)が倒れているのかを表す。</summary>
    public HashSet<int> dead_pins = new HashSet<int>();

    /// <summary>バトル場のピンが倒れているか（倒れているならTrue）</summary>
    public bool IsDead() => hpList[topIndex] < 1;

    //タイプ相性表、めっちゃ長いです
    private static int[,] typeArray = new int[10, 10] { { 2, 1, 4, 2, 2, 2, 2, 1, 1, 4 }, { 4, 2, 2, 1, 2, 2, 2, 1, 4, 2 }, { 1, 2, 2, 4, 2, 2, 2, 1, 1, 1 }, { 2, 4, 1, 2, 2, 2, 2, 1, 4, 2 }, { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }, { 2, 2, 2, 2, 2, 2, 4, 4, 1, 1 }, { 2, 2, 2, 2, 2, 4, 2, 4, 1, 4 }, { 1, 1, 1, 1, 2, 1, 1, 2, 2, 2 }, { 2, 1, 2, 1, 2, 2, 2, 2, 2, 2 }, { 1, 2, 4, 2, 2, 4, 1, 2, 2, 2 } };
    /*
        2 1 4 2 2 2 2 1 1 4
        4 2 2 1 2 2 2 1 4 2
        1 2 2 4 2 2 2 1 1 1
        2 4 1 2 2 2 2 1 4 2
        2 2 2 2 2 2 2 2 2 2
        2 2 2 2 2 2 4 4 1 1
        2 2 2 2 2 4 2 4 1 4
        1 1 1 1 2 1 1 2 2 2
        2 1 2 1 2 2 2 2 2 2
        1 2 4 2 2 4 1 2 2 2
    */

    //HPと攻撃の値をここで固定しています
    public void SetStatus()
    {
        for (int i = 0; i < 6; i++)
        {
            hpList[i] = pinsMaxHp[party[i]];
            attackList[i] = pinsAttack[party[i]];
        }
    }

    private float rest_choosing_time;
    public void ResetRestTime(float time) { rest_choosing_time = time; }
    public void ReduceRestTime(float time) { rest_choosing_time -= time; }
    public bool TimeOver() => rest_choosing_time < 0.0f;

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region UI

    //スタート時に出るオブジェクトの名前
    public GameObject topObject;

    //ピンのスタート位置
    public Vector3 pos;

    //HPバー管理
    public HpGauge topHpGauge;
    public HpSegment[] hpSegments;

    //pinのプレファブをここにアタッチ
    public GameObject[] pinPrefabs = new GameObject[10];

    //融合時のミニプレファブをここにアタッチ
    public GameObject[] miniPrefabs = new GameObject[10];

    //party[0]以降のUIを格納
    public GameObject[] partyUI = new GameObject[10];

    //canvas調整用
    [SerializeField] private GameObject canvas;

    //canvasの位置の調整用
    private Vector3 canvasPosition = new Vector3(960, 540, 0);

    //ボタンの位置
    public Transform[] buttonTransform = new Transform[6];
    public Button[] pinButtons = new Button[6];
    public GameObject[] actionButtons = new GameObject[5];
    public CanvasGroup canvasGroup;

    //初めに選出されたピンを所定の位置に生成し、スライダーをセットする関数
    public void AppearTopPin()
    {
        topObject = (GameObject)Instantiate(pinPrefabs[GetTopType()], pos, Quaternion.identity);
        topHpGauge.MaxHP = pinsMaxHp[GetTopType()];
        topHpGauge.CurrentHP = hpList[topIndex];
    }

    //パーティーのメインキャラ以外をUIにして表示
    public void AppearPartyUI()
    {
        for (int i = 0; i < 6; i++)
            Instantiate(partyUI[party[i]], buttonTransform[i]);
    }

    // パーティーのスライダーを初期化する
    public void SetSliders()
    {
        for (int i = 0; i < 6; i++)
        {
            hpSegments[i].MaxHP = pinsMaxHp[party[i]];
            hpSegments[i].CurrentHP = hpList[i];
        }
    }
    // 全てのスライダーのvalueを現在Hpに合わせる
    public void UpdateAllSliders()
    {
        topHpGauge.CurrentHP = hpList[topIndex];
        if (playerField.HasField(PlayerField.Merge))
        {
            int topMaxHP = pinsMaxHp[GetTopType()];
            foreach (int i in merged_pins)
                topMaxHP += pinsMaxHp[GetTypeByIndex(i)];
            topHpGauge.MaxHP = topMaxHP;
        }
        for (int i = 0; i < 6; i++)
            hpSegments[i].CurrentHP = hpList[i];
    }
    // 引数のindexのスライダーを表示
    public void ShowSlider(int _index)
    {
        hpSegments[_index].gameObject.SetActive(true);
    }
    // 引数のindexのスライダーを非表示
    public void HideSlider(int _index)
    {
        hpSegments[_index].gameObject.SetActive(false);
    }

    private IEnumerator ExecuteAnimation(IEnumerator target_animation)
    {
        TurnManager.animation_running = true;
        yield return target_animation;
        TurnManager.animation_running = false;
        yield break;
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Done By TurnManager

    // ゲームスタート時に行う処理
    public void Initialize()
    {
        ChangeThunderAttack();
        SetStatus();
        SetSliders();
        AppearTopPin();
        AppearPartyUI();
        HideSlider(topIndex);
        PinzAppear();
    }

    // 自分のターンの処理（PlayerActionで分岐）
    public void OwnTurn(Player opponent)
    {
        switch (playerAction)
        {
            case PlayerAction.Shield:
                if (CanShield())
                    Shield();
                break;
            case PlayerAction.Merge:
                if (CanMerge(opponent.playerField))
                {
                    Merge();
                    opponent.UpdateAllSliders();
                }
                break;
            case PlayerAction.Switch:
                Switch();
                break;
            case PlayerAction.Finisher:
                Finisher(opponent);
                opponent.UpdateAllSliders();
                break;
            case PlayerAction.Attacks:
                Attack(opponent);
                StartCoroutine(ExecuteAnimation(AnimationManager.instance.AttackAnimation(this, opponent)));
                break;
            case PlayerAction.ElemActions:
                ElemAction();
                opponent.UpdateAllSliders();
                break;
        }
        if (opponent.IsDead())
            opponent.dead_pins.Add(opponent.topIndex);
    }

    public void AfterOpponentTurn()
    {
        ReduceOrRemoveUndergrowth();
        ReduceOrRemoveWadatsumi();
        ReduceOrRemoveKusanagi();
    }

    // 両者のターンが終わったあとに行う処理
    public void TurnEnd()
    {
        RemoveGuard();
    }

    public void OwnEnd()
    {
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Shield

    // あと何回ガードできるか
    public int rest_guard = 3;
    // ガード状態にする、解除する
    public void AddGuard() => AddField(PlayerField.Guard);
    public void RemoveGuard() => RemoveField(PlayerField.Guard);
    /// <summary>
    /// ガードを消費していてガードがもうできないのか、まだ残っていてできるのか（できるならTrue、できないならFalse）
    /// </summary>
    public bool CanShield()
    {
        if (rest_guard < 1)
        {
            Debug.Log("ガードを消費し尽しました。もうガードできません。");
            return false;
        }
        return true;
    }
    // シールドする
    public void Shield()
    {
        rest_guard--;
        AddGuard();
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Merge

    /// <summary> ベースとなるピンに対して、partyの何番目が新たにマージされるのかを表す。 </summary>
    public HashSet<int> newly_merged_pins;
    /// <summary> 場の状態的に、今マージできるか（できるならTrue、できないならFalseが返ってくる）</summary>
    public bool CanMerge(PlayerField opponent_field)
    {
        if (opponent_field.HasField(PlayerField.Wadatsumi))
        {
            Debug.Log("ワダツミ状態のため融合できません");
            return false;
        }
        return true;
    }
    /// <summary> ベースとなるピンに対して、partyのindex番目をマージできるのかを返す（できるならTrue）。</summary>
    public bool CanMergedWithTop(int index)
    {
        if (dead_pins.Contains(index))
        {
            Debug.Log("このピンはすでに死んでいます");
            return false;
        }
        if (topIndex == index || merged_pins.Contains(index))
        {
            Debug.Log("このピンは現在バトル場に出ています。");
            return false;
        }
        // バトルフィールドのピンが無または同じタイプならOK
        return (GetTopType() == 4 || GetTopType() == GetTypeByIndex(index));
    }

    public GameObject[] miniObject = new GameObject[5];
    // 融合する
    public void Merge()
    {
        AddField(PlayerField.Merge);
        // クサナギ状態ならratioを2にして、そうじゃなければ1にする
        int ratio = playerField.HasField(PlayerField.Kusanagi) ? 2 : 1;
        foreach (int merged_pin_index in newly_merged_pins)
        {
            // topにmergeできないならcontinue
            if (!CanMergedWithTop(merged_pin_index))
                continue;
            merged_pins.Add(merged_pin_index);
            hpList[topIndex] += (hpList[merged_pin_index] * ratio);
            hpList[merged_pin_index] = 0;
            PinzAppear(merged_pin_index);
            // マージするピンのUIを非表示
            HideSlider(merged_pin_index);
        }
        UpdateAllSliders();

        for (int i = 0; i < 5; i++)
            Destroy(miniObject[i]);
        for (int i = 0; i < merged_pins.Count; i++)
            miniObject[i] = (GameObject)Instantiate(miniPrefabs[party[merged_pins.ToArray()[i]]], pos, Quaternion.identity);
    }
    /// <summary>
    /// 自分の融合を解除
    /// </summary>
    /// <returns>解除のときに等分したHp(divided_hp)</returns>
    public int SeparateMerge()
    {
        RemoveField(PlayerField.Merge);
        // Hpを等分
        int divided_hp = hpList[topIndex] / (merged_pins.Count + 1);
        hpList[topIndex] = divided_hp;
        // 相手がクサナギ状態ならベンチに戻されるピンのHpはさらに半分となる
        int ratio = playerField.HasField(PlayerField.Kusanagi) ? 2 : 1;
        foreach (int index in merged_pins)
        {
            hpList[index] = divided_hp / ratio;
            // もし死んでいるなら、dead_pinsに登録
            if (hpList[index] < 1)
                dead_pins.Add(index);
            PinzDisappear(index);
            ShowSlider(index);
        }
        merged_pins.Clear();
        UpdateAllSliders();

        for (int i = 0; i < 5; i++)
            Destroy(miniObject[i]);

        return divided_hp;
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Switch

    // 何番目のピンと交換するのか
    public int change_index;

    /// <summary> partyのindex番目を交換できるのかを返す（できるならTrue）。</summary>
    public bool CanSwitch(int index)
    {
        if (dead_pins.Contains(index))
        {
            Debug.Log("このピンはすでに死んでいます");
            return false;
        }
        // 今出てるピン、または融合しているピンとは交換できない
        if (index == topIndex || merged_pins.Contains(index))
        {
            Debug.Log("このピンは現在バトル場に出ています。違うピンを選択してください。");
            return false;
        }
        return true;
    }

    // 交換
    public void Switch()
    {
        // 現在、場に出ているピンのHp
        int top_pin_hp = hpList[topIndex];
        // もし融合しているなら解除する
        if (playerField.HasField(PlayerField.Merge))
            top_pin_hp = SeparateMerge();
        // 交換でベンチに戻るピンが死んでいるなら、dead_pinsに登録
        if (top_pin_hp < 1)
            dead_pins.Add(topIndex);
        // 新しく出てくるはずのピンが既に死んでいたら、違うピンを適当に選ぶ
        if (hpList[change_index] < 1)
        {
            for (int i = 0; i < 6; i++)
            {
                if (dead_pins.Contains(i))
                    continue;
                else
                {
                    change_index = i;
                    break;
                }
            }
        }
        // 交換する前に今のtopIndexのスライダーを表示しておく
        ShowSlider(topIndex);
        Destroy(topObject);
        PinzDisappear();
        ChangeTopIndex();
        // 新しく出たtopIndexのスライダーは非表示
        HideSlider(topIndex);
        AppearTopPin();
        PinzAppear();
    }

    /// <summary>自分のピンズがバトル場に出た直後の処理</summary>
    public void PinzAppear() { PinzAppear(topIndex); }

    /// <summary>
    /// 自分のピンズがバトル場から退避する直前の処理
    /// </summary>
    /// <param name="index">バトル場に出るピンのindex</param>
    public void PinzAppear(int index)
    {
        int type = GetTypeByIndex(index);
        // 炎タイプなら貫通をFieldに追加
        if (type == 0)
            AddNullifyDefense();
    }

    /// <summary>自分のピンズがバトル場から退避する直前の処理</summary>
    public void PinzDisappear() { PinzDisappear(topIndex); }

    /// <summary>
    /// 自分のピンズがバトル場から退避する直前の処理
    /// </summary>
    /// <param name="index">バトル場に出るピンのindex</param>
    public void PinzDisappear(int index)
    {
        int type = GetTypeByIndex(index);
        if (type == 0)
            RemoveNullifyDefense();
    }

    // topIndexにchange_indexを代入するだけ
    public void ChangeTopIndex() => topIndex = change_index;

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Finisher

    private bool can_finisher = true;
    public void Finisher(Player opponent)
    {
        if (!can_finisher)
            return;
        can_finisher = false;
        switch (GetTopType())
        {
            case 0: Homura(opponent); break;
            case 1: Wadatsumi(opponent); break;
            case 2: Kusanagi(opponent); break;
            case 3: Ikazuchi(opponent); break;
        }
    }
    // 炎のヒギ（融合を望まないなら newly_merged_pins = new List<int>(0); としてください）
    public void Homura(Player opponent)
    {
        if (newly_merged_pins.Count > 0 && CanMerge(opponent.playerField))
            Merge();
        if (opponent.playerField.HasField(PlayerField.Guard))
            Debug.Log("ガードによって防がれた！");
        else
            opponent.hpList[opponent.topIndex] -= 16 * (typeArray[0, opponent.GetTopType()] / 2);
    }
    // 水のヒギ
    private int rest_wadatsumi_turn = 0;
    public void Wadatsumi(Player opponent)
    {
        rest_wadatsumi_turn = 3;
        AddField(PlayerField.Wadatsumi);
        // 相手が融合状態でガードしていないなら、相手の融合を解除
        if (opponent.playerField.HasField(PlayerField.Merge))
        {
            if (opponent.playerField.HasField(PlayerField.Guard))
                Debug.Log("ガードによって防がれた！");
            else
                opponent.SeparateMerge();
        }
    }
    // ワダツミのターンを減らしたり、解除したり（すでに効果切れは何もしない）
    public void ReduceOrRemoveWadatsumi()
    {
        if (rest_wadatsumi_turn < 1)
            return;
        rest_wadatsumi_turn--;
        if (rest_wadatsumi_turn < 1)
            RemoveField(PlayerField.Wadatsumi);
    }
    // 木のヒギ（融合を望まないなら newly_merged_pins = new List<int>(0); としてください。
    // 融合以外の行動をとりたくないなら wood_action = PlayerAction.None; としてください。）
    public PlayerAction wood_action = PlayerAction.None;
    private int rest_kusanagi_turn = 0;
    public void Kusanagi(Player opponent)
    {
        rest_kusanagi_turn = 3;
        AddField(PlayerField.Kusanagi);
        // Hpを最大まで回復し、2倍にする
        hpList[topIndex] = pinsMaxHp[GetTopType()];
        hpList[topIndex] *= 2;
        // 融合できるならする
        if (newly_merged_pins != null && newly_merged_pins.Count > 0 && CanMerge(opponent.playerField) && newly_merged_pins.Count < 3)
            Merge();
        // 行動できるならする
        if (wood_action != PlayerAction.None &&
            wood_action != PlayerAction.Shield &&
            wood_action != PlayerAction.Merge &&
            wood_action != PlayerAction.Finisher)
            SetPlayerAction(wood_action);
        OwnTurn(opponent);
    }
    // クサナギのターンを減らしたり、解除したり（Hp2倍も解除。すでに効果切れは何もしない）
    public void ReduceOrRemoveKusanagi()
    {
        if (rest_kusanagi_turn < 1)
            return;
        rest_kusanagi_turn--;
        if (rest_kusanagi_turn < 1)
        {
            RemoveField(PlayerField.Kusanagi);
            hpList[topIndex] /= 2;
        }
    }
    // 雷のヒギ
    // 雷のヒギでベンチ攻撃する相手のピンのindex
    public int ikazuchi_index;
    public void Ikazuchi(Player opponent)
    {
        if (opponent.playerField.HasField(PlayerField.Guard))
        {
            Debug.Log("ガードによって防がれた！");
            return;
        }
        int damage = (thunder_pinz_amount + opponent.thunder_pinz_amount) * 2;
        opponent.hpList[opponent.topIndex] -= damage * (typeArray[3, opponent.GetTopType()] / 2);
        opponent.hpList[ikazuchi_index] -= damage * (typeArray[3, opponent.party[ikazuchi_index]] / 2);
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Damage

    public void Attack(Player opponent)
    {
        if (opponent.playerField.HasField(PlayerField.Guard))
        {
            Debug.Log("相手のガードによって防がれた！！");
            return;
        }

        int damage = attackList[topIndex];
        if (playerField.HasField(PlayerField.Merge))
            foreach (int i in merged_pins)
                damage += attackList[i];
        // 相手のフィールドが草壁で、自分に貫通があるならそのままで、ないなら半分
        if (opponent.playerField.HasField(PlayerField.Undergrowth))
            damage = playerField.HasField(PlayerField.NullifyDef) ? damage : damage / 2;
        // 相手がワダツミ状態ならダメージを2にする
        if (opponent.playerField.HasField(PlayerField.Wadatsumi))
            damage = 2;

        // 相手が自分の攻撃によってダメージを受ける
        opponent.OnDamage(damage, GetTopType());

        // 自分が炎で相手が融合しているなら+2ダメージ
        if (GetTopType() == 0 && opponent.playerField.HasField(PlayerField.Merge))
            opponent.OnFixedDamage(2);
    }

    // ダメージを与える関数ではなく、ダメージを受ける関数にしました
    public void OnDamage(int damage, int opponent_pin_type)
    {
        hpList[topIndex] -= damage * typeArray[opponent_pin_type, GetTopType()] / 2;
    }
    // 固定ダメージ（タイプ相性を考慮しない）関数
    public void OnFixedDamage(int damage)
    {
        hpList[topIndex] -= damage;
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Element Specific Abilities

    // 炎の固有能力
    public void AddNullifyDefense() => AddField(PlayerField.NullifyDef);
    public void RemoveNullifyDefense() => RemoveField(PlayerField.NullifyDef);

    //雷の攻撃力を可変にしてるます
    public int thunder_pinz_amount = 0;
    public void ChangeThunderAttack()
    {
        for (int j = 0; j < 6; j++)
            if (party[j] == 3) thunder_pinz_amount++;
        switch (thunder_pinz_amount)
        {
            case 0: pinsAttack[3] = 1; break;
            case 1: pinsAttack[3] = 2; break;
            case 2: pinsAttack[3] = 4; break;
            case 3: pinsAttack[3] = 8; break;
            case 4: pinsAttack[3] = 1; break;
        }
    }

    public void ChangeAbyssAttack()
    {

    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Element Specific Actions

    public int[] elem_action_amounts = new int[10] { 0, 1, 1, 0, 0, 0, 0, 5, 1, 0 };

    // 無に融合したピンのうち、固有行動が存在するピンのタイプを抽出したもの
    public List<int> norm_action_type_list = new List<int>();

    // 融合されているピンの内、固有行動が存在するピンのタイプを抽出してListにまとめる
    public void ExtractTypeList()
    {
        List<int> type_list = new List<int>();
        foreach (var i in merged_pins)
        {
            if (elem_action_amounts[party[i]] != 0 && !type_list.Contains(party[i]))
                type_list.Add(party[i]);
        }
        norm_action_type_list = type_list;
    }

    private int norm_action_type;
    public int SetNormActionType(int index)
    {
        return norm_action_type = norm_action_type_list[index];
    }

    public void ElemAction()
    {
        int switch_num = GetTopType();
        if (switch_num == 4)
            switch_num = norm_action_type;

        switch (switch_num)
        {
            case 1: Regenerate(regenerate_selected_index); break;
            case 2: Kusakabe(); break;
            case 7:
                switch (storm_elem_action_index)
                {
                    case 0: Kazakami(); break;
                    case 1: Bakufu(); break;
                    case 2: Fukitobashi(); break;
                    case 3: Tatsumaki(); break;
                    case 4: Kazaana(); break;
                    default: break;
                }
                break;
            case 8: Koujin(); break;
            default: break;
        }
    }

    [System.NonSerialized] public int regenerate_selected_index;
    public void Regenerate(int index)
    {
        if (!(-1 < index && index < 6))
        {
            Debug.Log("indexが範囲外です");
            return;
        }
        if (!dead_pins.Contains(index))
        {
            Debug.Log("このピンは生きています");
            return;
        }
        dead_pins.Remove(index);
        hpList[index] = pinsMaxHp[GetTypeByIndex(index)];
        StartCoroutine(ExecuteAnimation(hpSegments[index].HPTween(hpList[index])));
        Debug.Log("再生");
    }

    private int rest_undergrowth_turn = 0;
    public void Kusakabe()
    {
        rest_undergrowth_turn = 3;
        AddField(PlayerField.Undergrowth);
        Debug.Log("草壁");
    }
    // 草壁のターンを減らしたり、解除したり（すでに効果切れは何もしない）
    public void ReduceOrRemoveUndergrowth()
    {
        if (rest_undergrowth_turn < 1)
            return;
        rest_undergrowth_turn--;
        if (rest_undergrowth_turn < 1)
            RemoveField(PlayerField.Undergrowth);
    }

    public void Seiiki()
    {
        //
    }

    [System.NonSerialized] public int storm_elem_action_index;
    public void Kazakami()
    {
        // Debug.Log("1");
        // AppearSubattackButton(false);
        Debug.Log("風上");
    }

    // public void Bakufuu()
    public void Bakufu()
    {
        // Debug.Log("2");
        // AppearSubattackButton(false);
        Debug.Log("爆風");
    }

    public void Fukitobashi()
    {
        // Debug.Log("3");
        // AppearSubattackButton(false);
        Debug.Log("吹き飛ばし");
    }

    // public void Tatumaki()
    public void Tatsumaki()
    {
        // Debug.Log("4");
        // AppearSubattackButton(false);
        Debug.Log("竜巻");
    }

    // public void Kazeana()
    public void Kazaana()
    {
        // Debug.Log("5");
        // AppearSubattackButton(false);
        Debug.Log("風穴");
    }

    public void Koujin()
    {
        //
        // AppearSubattackButton(false);
        Debug.Log("鉱塵");
    }

    #endregion
}
