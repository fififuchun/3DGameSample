using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Mirror;
using Mergepins;
using Mergepins.Network;

public class TurnManager : MonoBehaviour
{
    private GameState gameState = GameState.Initial;

    public int turnNumber { get; private set; }

    private bool my_is_first = default;

    public static bool animation_running = false;

    public static bool finish_choose = false;

    private float choosing_time = 20.0f;

    public void StartLoop()
    {
        if (!(gameState == GameState.Initial))
            return;
        Debug.Log("Initialized");
        turnNumber = 0;
        UIManager.instance.turnCounter.text = turnNumber.ToString();
        gameState = GameState.Wait;
    }

    private void GoNextAndDoMethods()
    {
        switch (gameState)
        {
            case GameState.Judge:
                if (my_is_first)
                {
                    my.OwnTurn(enemy);
                    gameState = GameState.MyTurn;
                }
                else
                {
                    enemy.OwnTurn(my);
                    gameState = GameState.EnemyTurn;
                }
                break;
            case GameState.MyTurn:
                enemy.AfterOpponentTurn();
                gameState = GameState.EnemyTurnChoose;
                enemy.ResetRestTime(choosing_time);
                break;
            case GameState.EnemyTurn:
                my.AfterOpponentTurn();
                gameState = GameState.MyTurnChoose;
                my.ResetRestTime(choosing_time);
                UIManager.instance.StartChoose();
                break;
            case GameState.MyTurnChoose:
                UIManager.instance.FinishChoose();
                if (my_is_first)
                {
                    if (my.IsDead())
                    {
                        my.SetPlayerAction(PlayerAction.Switch);
                        my.OwnTurn(enemy);
                    }
                    gameState = GameState.MySwitch;
                }
                else
                {
                    if (my.IsDead())
                        my.SetPlayerAction(PlayerAction.Switch);
                    my.OwnTurn(enemy);
                    gameState = GameState.MyTurn;
                }
                break;
            case GameState.EnemyTurnChoose:
                if (my_is_first)
                {
                    if (enemy.IsDead())
                        enemy.SetPlayerAction(PlayerAction.Switch);
                    enemy.OwnTurn(my);
                    gameState = GameState.EnemyTurn;
                }
                else
                {
                    if (enemy.IsDead())
                    {
                        enemy.SetPlayerAction(PlayerAction.Switch);
                        enemy.OwnTurn(my);
                    }
                    gameState = GameState.EnemySwitch;
                }
                break;
            case GameState.MySwitch:
                my.TurnEnd();
                enemy.TurnEnd();
                gameState = GameState.TurnEnd;
                break;
            case GameState.EnemySwitch:
                my.TurnEnd();
                enemy.TurnEnd();
                gameState = GameState.TurnEnd;
                break;
            case GameState.TurnEnd:
                if (my_is_first)
                {
                    my.OwnEnd();
                    gameState = GameState.MyEnd;
                }
                else
                {
                    enemy.OwnEnd();
                    gameState = GameState.EnemyEnd;
                }
                break;
            case GameState.MyEnd:
                gameState = GameState.MyEndChoose;
                my.ResetRestTime(choosing_time);
                break;
            case GameState.MyEndChoose:
                if (my_is_first)
                {
                    enemy.OwnEnd();
                    gameState = GameState.EnemyEnd;
                }
                else
                {
                    gameState = GameState.Complete;
                    MatchController.instance.CmdLoopEnded();
                }
                break;
            case GameState.EnemyEnd:
                gameState = GameState.EnemyEndChoose;
                enemy.ResetRestTime(choosing_time);
                break;
            case GameState.EnemyEndChoose:
                if (my_is_first)
                {
                    gameState = GameState.Complete;
                    MatchController.instance.CmdLoopEnded();
                }
                else
                {
                    my.OwnEnd();
                    gameState = GameState.MyEnd;
                }
                break;
        }
    }

    public void BackToWait()
    {
        if (!(gameState == GameState.Complete))
            return;
        my.SetPlayerAction(PlayerAction.None);
        enemy.SetPlayerAction(PlayerAction.None);
        turnNumber++;
        UIManager.instance.turnCounter.text = turnNumber.ToString();
        gameState = GameState.Wait;
    }

    public void RequestRegisterPlayerAction(MatchPlayerAction action)
    {
        if (gameState != GameState.Wait) return;
        MatchController.instance.CmdRegisterPlayerAction(action);
    }

    public void GoFromWaitToNext(bool random)
    {
        if (!(gameState == GameState.Wait) || my.playerAction == PlayerAction.None || enemy.playerAction == PlayerAction.None)
            return;
        if ((my.playerAction == enemy.playerAction) || (my.playerAction >= PlayerAction.Attacks && enemy.playerAction >= PlayerAction.Attacks))
            my_is_first = random;
        else
            my_is_first = my.playerAction < enemy.playerAction;
        Debug.Log($"my_is_first = {my_is_first}");
        gameState = GameState.Judge;
    }

    public void EndGame()
    {
        Destroy(my.topObject);
        Destroy(enemy.topObject);
        for (int i = 0; i < 5; i++)
        {
            Destroy(my.miniObject[i]);
            Destroy(enemy.miniObject[i]);
        }
    }

    //おまじない
    public Player my;
    public Player enemy;

    //他スクリプトから参照できるようにするおまじない
    public static TurnManager instance;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // my.party = TeamManager.instance?.team.memberList.ToArray() ?? new int[6] { 4, 7, 2, 2, 7, 3 };
        // enemy.party = new int[6] { 0, 1, 1, 2, 2, 3 };
        // my.topIndex = 0;
        // enemy.topIndex = Random.Range(0, 6);
    }

    public void StartGame()
    {
        my.pos = new Vector3(52, 3, -33);
        enemy.pos = new Vector3(32, 3, -45);

        my.Initialize();
        enemy.Initialize();
        StartLoop();
    }

    void FixedUpdate()
    {
        switch (gameState)
        {
            case GameState.Judge:
                if (animation_running)
                    break;
                GoNextAndDoMethods();
                break;
            case GameState.MyTurn:
                if (animation_running)
                    break;
                GoNextAndDoMethods();
                break;
            case GameState.EnemyTurn:
                if (animation_running)
                    break;
                GoNextAndDoMethods();
                break;
            case GameState.MyTurnChoose:
                if (my.IsDead())
                {
                    if (finish_choose || my.TimeOver())
                    {
                        GoNextAndDoMethods();
                    }
                    else
                    {
                        my.ReduceRestTime(Time.deltaTime);
                    }
                }
                else
                    GoNextAndDoMethods();
                break;
            case GameState.EnemyTurnChoose:
                if (enemy.IsDead())
                    Debug.Log("敵が倒れた。対戦相手が次のピンを選んでいる…");
                else
                    GoNextAndDoMethods();
                break;
            case GameState.MySwitch:
                if (my.IsDead())
                    Debug.Log("My側の死に出しのアニメーション");
                else
                    GoNextAndDoMethods();
                break;
            case GameState.EnemySwitch:
                if (enemy.IsDead())
                    Debug.Log("Enemy側の死に出しのアニメーション");
                else
                    GoNextAndDoMethods();
                break;
            case GameState.TurnEnd:
                Debug.Log("ガードが消える等のアニメーション");
                GoNextAndDoMethods();
                break;
            case GameState.MyEnd:
                Debug.Log("鉱陣等のアニメーション");
                GoNextAndDoMethods();
                break;
            case GameState.MyEndChoose:
                if (my.IsDead())
                    Debug.Log("自分のピンが倒れた。次のピンを選んで！");
                else
                    GoNextAndDoMethods();
                break;
            case GameState.EnemyEnd:
                Debug.Log("鉱陣等のアニメーション");
                GoNextAndDoMethods();
                break;
            case GameState.EnemyEndChoose:
                if (enemy.IsDead())
                    Debug.Log("敵が倒れた。対戦相手が次のピンを選んでいる…");
                else
                    GoNextAndDoMethods();
                break;
            case GameState.Complete:
                break;
        }
    }

    // EnemyAttackのボタンにアタッチ(一時的に)
    private int enemyattackButtonIndex = 0;
    public void PushEnemyAttackButton()
    {
        switch (enemyattackButtonIndex)
        {
            case 0:
                enemyattackButtonIndex = 1;
                break;
            case 1:
                enemy.SetPlayerAction(PlayerAction.Attacks);
                enemyattackButtonIndex = 0;
                break;
        }
    }
}