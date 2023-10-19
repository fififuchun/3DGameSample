using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Mergepins;
using Mergepins.Network;

public class UIManager : MonoBehaviour, IPointerClickHandler
{
    public static UIManager instance;
    private void Awake() { instance = this; }

    // changeボタンのCanvasGroup
    public CanvasGroup canvasGroup;
    public Image actionButtonImage;
    public Sprite actionSprite;
    public Sprite attackSprite;
    private string[] aqua_name = new string[1] { "再生" };
    private string[] wood_name = new string[1] { "再生" };
    private string[] storm_name = new string[5] { "風上", "爆風", "吹き飛ばし", "竜巻", "風穴" };
    private string[] crystal_name = new string[1] { "再生" };

    public Button[] actionButtons = new Button[5];
    public Image[] actionButtonImages = new Image[5];

    public Sprite RegenerateSprite;
    public Sprite UndergrowthSprite;
    public Sprite[] StormActionSprites = new Sprite[5];
    public Sprite AbsorbSprite;
    public Sprite MinesSprite;

    public Image finisherButtonImage;
    public Sprite[] finisherSprites = new Sprite[10];
    public Image higiButtonImageOn;
    public Image higiButtonImageOff;
    public Image higiButtonLetter;

    // ピンのUIの親のボタン
    public Button[] PinUIButtons = new Button[6];

    // 今何ターン目かを表示するテキスト
    public Text turnCounter;

    // 今どんなフィールドが出ているかを右下に出すテキスト
    public Text fieldInProgress;
    private string[] fieldList = new string[6] { "ガード", "融合中", "貫通", "海神", "草壁", "草薙" };
    private void ShowFieldInProgress()
    {
        fieldInProgress.text = "";
        bool[] bool_array = my.playerField.ToBoolArray();
        for (int i = 0; i < 6; i++)
            if (bool_array[i])
                fieldInProgress.text += fieldList[i];
    }

    private float theta = 0;
    private void MoveMergedPins()
    {
        theta += Time.deltaTime;
        int num = my.merged_pins.Count;
        for (int i = 0; i < num; i++)
        {
            float pos_theta = theta + (2 * Mathf.PI / num) * i;
            my.miniObject[i].transform.position = 3 * new Vector3(Mathf.Cos(pos_theta), -1f / 3f, Mathf.Sin(pos_theta)) + my.pos;
        }
    }

    public Player my;

    private void Start()
    {
        DisablePinUIButton();
    }

    private void Update()
    {
        if (my.playerField.HasField(PlayerField.Merge)) MoveMergedPins();
    }

    public void OnPlayerFieldAdded()
    {
        ShowFieldInProgress();
    }

    public void OnPlayerFieldRemoved()
    {
        ShowFieldInProgress();
    }

    // IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        ResetPushedButtons();
    }

    /// <summary>
    /// すでに、あるボタンが押されているとき(例えばActionButtonがすでに押されていて、SubattackButtonが出ているとき)
    /// そのボタンを閉じる(リセットする)関数
    /// </summary>
    public void ResetPushedButtons()
    {
        if (merge_already_pushed) ResetMergeButton();
        if (switch_already_pushed) ResetSwitchButton();
        if (higi_already_pushed) ResetHigiButton();
        if (action_already_pushed) ResetActionButton();
        if (regenerate_already_pushed) ResetRegenerateButton();
    }

    //----------------------------------------------------------------------------------------------------
    #region Pin UI Button

    private void EnablePinUIButton()
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
    }

    private void DisablePinUIButton()
    {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0.75f;
        canvasGroup.blocksRaycasts = false;
    }

    [SerializeField] private Text[] selected_texts = new Text[6];
    private int selected_index = 0;
    // ラジオボタン
    private void RadioPinUIButton(int index)
    {
        // いまSelectedになっているやつをSelectedじゃなくする
        selected_texts[selected_index].enabled = false;
        selected_index = index;
        selected_texts[index].enabled = true;
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Shield Button

    public void PushShieldButton()
    {
        ResetPushedButtons();
        if (!my.CanShield())
        {
            Debug.Log("ガードを消費し尽しました。もうガードできません。");
            return;
        }
        TurnManager.instance.RequestRegisterPlayerAction(
            new MatchPlayerAction
            {
                playerAction = PlayerAction.Shield
            });
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Merge Button

    private bool merge_already_pushed = false;

    // mergeボタンにこれを登録
    public void PushMergeButton()
    {
        if (merge_already_pushed)// 2回目押したときの挙動
        {
            // 融合できない場合は融合できないようにする
            if (!my.CanMergedWithTop(selected_index))
            {
                Debug.Log("融合できません。");
                return;
            }
            TurnManager.instance.RequestRegisterPlayerAction(
                new MatchPlayerAction
                {
                    playerAction = PlayerAction.Merge,
                    newly_merged_pins = new int[] { selected_index }
                });
            ResetMergeButton();
        }
        else// 1回目押したときの挙動
        {
            ResetPushedButtons();
            merge_already_pushed = true;
            EnablePinUIButton();
            selected_index = my.topIndex;
            selected_texts[selected_index].enabled = true;
            for (int i = 0; i < 6; i++)
            {
                int j = i;
                PinUIButtons[i].onClick.AddListener(() => RadioPinUIButton(j));
            }
        }
    }

    // 1回目押したときの挙動とおよそ逆の挙動
    public void ResetMergeButton()
    {
        merge_already_pushed = false;
        DisablePinUIButton();
        selected_texts[selected_index].enabled = false;
        for (int i = 0; i < 6; i++)
            PinUIButtons[i].onClick.RemoveAllListeners();
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Switch Button

    private bool switch_already_pushed = false;

    /// <summary>SwitchButtを押して交換の行動を選択したときに行うコールバック</summary>
    public UnityEvent PushSwitchCallback = new UnityEvent();

    // changeボタンにこれを登録
    public void PushSwitchButton()
    {
        if (switch_already_pushed)// 2回目押したときの挙動
        {
            // 今出てるピン、または融合しているピンと交換できないようにする
            if (!my.CanSwitch(selected_index))
            {
                Debug.Log("交換できません。");
                return;
            }
            TurnManager.instance.RequestRegisterPlayerAction(
                new MatchPlayerAction
                {
                    playerAction = PlayerAction.Switch,
                    change_index = selected_index
                });
            PushSwitchCallback?.Invoke();
            ResetSwitchButton();
        }
        else// 1回目押したときの挙動
        {
            ResetPushedButtons();
            switch_already_pushed = true;
            EnablePinUIButton();
            selected_index = my.topIndex;
            selected_texts[selected_index].enabled = true;
            for (int i = 0; i < 6; i++)
            {
                int j = i;
                PinUIButtons[i].onClick.AddListener(() => RadioPinUIButton(j));
            }
        }
    }

    // 1回目押したときの挙動とおよそ逆の挙動
    private void ResetSwitchButton()
    {
        switch_already_pushed = false;
        DisablePinUIButton();
        selected_texts[selected_index].enabled = false;
        for (int i = 0; i < 6; i++)
            PinUIButtons[i].onClick.RemoveAllListeners();
    }

    [SerializeField] Image blockPanel;
    /// <summary>死に出しの選択のUIを起動</summary>
    public void StartChoose()
    {
        blockPanel.raycastTarget = true;
        if (!switch_already_pushed) PushSwitchButton();
        TurnManager.finish_choose = false;
        PushSwitchCallback.AddListener(TrueFinishChoose);
    }
    /// <summary>死に出しの選択のUIを修了</summary>
    public void FinishChoose()
    {
        blockPanel.raycastTarget = false;
        if (switch_already_pushed) ResetSwitchButton();
        PushSwitchCallback.RemoveListener(TrueFinishChoose);
    }
    private void TrueFinishChoose() { TurnManager.finish_choose = true; }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Finisher Button

    public void PushFinisherButton()
    {
        ResetHigiButton();
        higiButtonImageOff.enabled = false;
        higiButtonLetter.enabled = false;
        TurnManager.instance.RequestRegisterPlayerAction(
            new MatchPlayerAction
            {
                playerAction = PlayerAction.Finisher
            }
        );
    }

    private bool higi_already_pushed = false;
    public void PushHigiButton()
    {
        if (higi_already_pushed)// 2回目押したときの挙動
        {
            ResetHigiButton();
        }
        else// 1回目押したときの挙動
        {
            ResetPushedButtons();
            finisherButtonImage.sprite = finisherSprites[my.GetTopType()];
            higi_already_pushed = true;
            actionButtonImage.enabled = false;
            finisherButtonImage.enabled = true;
            higiButtonImageOn.enabled = true;
            higiButtonImageOff.enabled = false;
        }
    }
    private void ResetHigiButton()
    {
        higi_already_pushed = false;
        actionButtonImage.enabled = true;
        finisherButtonImage.enabled = false;
        higiButtonImageOn.enabled = false;
        higiButtonImageOff.enabled = true;
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Action Button

    private bool action_already_pushed = false;

    private int subattack_button_amount = 0;

    private void HideSubattackButton()
    {
        for (int i = 0; i < subattack_button_amount; i++)
            actionButtonImages[i].enabled = false;
    }

    private void ResetActionButton()
    {
        HideSubattackButton();
        subattack_button_amount = 0;
        action_already_pushed = false;
        actionButtonImage.sprite = actionSprite;
    }

    private void AppearSubattackButton(int type)
    {
        subattack_button_amount = my.elem_action_amounts[type];
        for (int i = 0; i < subattack_button_amount; i++)
        {
            actionButtons[i].onClick.RemoveAllListeners();
            actionButtonImages[i].enabled = true;
            int j = i;
            switch (type)
            {
                case 1:
                    actionButtonImages[i].sprite = RegenerateSprite;
                    actionButtons[i].onClick.AddListener(() => RegenerateButton());
                    break;
                case 2:
                    actionButtonImages[i].sprite = UndergrowthSprite;
                    actionButtons[i].onClick.AddListener(() =>
                    {
                        TurnManager.instance.RequestRegisterPlayerAction(
                            new MatchPlayerAction
                            {
                                playerAction = PlayerAction.ElemActions,
                                norm_action_type = norm_action_type
                            });
                        ResetActionButton();
                    });
                    break;
                case 7:
                    for (int k = 0; k < 5; k++)
                        actionButtonImages[k].sprite = StormActionSprites[k];
                    actionButtons[i].onClick.AddListener(() =>
                    {
                        TurnManager.instance.RequestRegisterPlayerAction(
                            new MatchPlayerAction
                            {
                                playerAction = PlayerAction.ElemActions,
                                storm_elem_action_index = j,
                                norm_action_type = norm_action_type
                            });
                        ResetActionButton();
                    });
                    break;
                default:
                    actionButtons[i].onClick.AddListener(() =>
                    {
                        TurnManager.instance.RequestRegisterPlayerAction(
                            new MatchPlayerAction
                            {
                                playerAction = PlayerAction.ElemActions,
                                norm_action_type = norm_action_type
                            });
                        ResetActionButton();
                    });
                    break;
            }
        }
    }

    int norm_action_type;
    private void AppearNormSubattackButton()
    {
        my.ExtractTypeList();
        subattack_button_amount = my.norm_action_type_list.Count;
        for (int i = 0; i < subattack_button_amount; i++)
        {
            actionButtons[i].onClick.RemoveAllListeners();
            actionButtonImages[i].enabled = true;
            int j = i;
            actionButtons[i].onClick.AddListener(() =>
            {
                HideSubattackButton();
                norm_action_type = my.norm_action_type_list[j];
                AppearSubattackButton(norm_action_type);
            });
        }
    }

    // これをボタンに登録
    public void PushActionButton()
    {
        int type = my.GetTopType();
        if (type == 4)// 出ているピンが無タイプだった時は特殊処理
        {
            if (!action_already_pushed)// 1回目押したときの挙動
            {
                ResetPushedButtons();
                action_already_pushed = true;
                actionButtonImage.sprite = attackSprite;
                AppearNormSubattackButton();
            }
            else// 2回目押したときの挙動
            {
                TurnManager.instance.RequestRegisterPlayerAction(
                    new MatchPlayerAction { playerAction = PlayerAction.Attacks }
                );
                ResetActionButton();
            }
        }
        else
        {
            if (!action_already_pushed)// 1回目押したときの挙動
            {
                ResetPushedButtons();
                action_already_pushed = true;
                actionButtonImage.sprite = attackSprite;
                AppearSubattackButton(type);
            }
            else// 2回目押したときの挙動
            {
                TurnManager.instance.RequestRegisterPlayerAction(
                    new MatchPlayerAction { playerAction = PlayerAction.Attacks }
                );
                ResetActionButton();
            }
        }
    }

    #endregion
    //----------------------------------------------------------------------------------------------------
    #region Element Action Button

    private bool regenerate_already_pushed = false;
    // 水の固有行動のボタン
    private void RegenerateButton()
    {
        if (regenerate_already_pushed)// 2回目押したときの挙動
        {
            if (!my.dead_pins.Contains(selected_index))
            {
                Debug.Log("このピンはまだ生きています。");
                return;
            }
            TurnManager.instance.RequestRegisterPlayerAction(
                new MatchPlayerAction
                {
                    playerAction = PlayerAction.ElemActions,
                    regenerate_selected_index = selected_index
                }
            );
            ResetRegenerateButton();
            ResetActionButton();
        }
        else// 1回目押したときの挙動
        {
            regenerate_already_pushed = true;
            EnablePinUIButton();
            selected_index = my.topIndex;
            selected_texts[selected_index].enabled = true;
            for (int i = 0; i < 6; i++)
            {
                int j = i;
                PinUIButtons[i].onClick.AddListener(() => RadioPinUIButton(j));
            }
        }
    }
    // 1回目押したときの挙動とおよそ逆の挙動
    public void ResetRegenerateButton()
    {
        regenerate_already_pushed = false;
        DisablePinUIButton();
        selected_texts[selected_index].enabled = false;
        for (int i = 0; i < 6; i++)
            PinUIButtons[i].onClick.RemoveAllListeners();
    }

    #endregion
}
