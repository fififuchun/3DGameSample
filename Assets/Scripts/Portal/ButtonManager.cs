using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    private const string TEAM_NOT_EXIST_MESSAGE =
    "チームが存在しません。\nチーム編集画面からチームを作成してください。";
    private const string TEAM_MEMBER_NOT_ENOUGH_MESSAGE =
    "選択したチームはピンの数が足りていません。\nチーム編集画面から6体のピンを選択してください。";
    private const string TEAM_MEMBER_ERROR_MESSAGE =
    "編集したチームはバトルの出場要件を満たしていません。\nチーム編集画面から再度チームを編集してください。";
    public WarningDisplayer warning;

    public void BattleButton()
    {
        if (TeamManager.instance.team.memberList.Count == 0)
        {
            warning.DisplayWarning(TEAM_NOT_EXIST_MESSAGE);
            return;
        }
        if (TeamManager.instance.team.memberList.Count < 6)
        {
            warning.DisplayWarning(TEAM_MEMBER_NOT_ENOUGH_MESSAGE);
            return;
        }
        if (TeamManager.instance.team.memberList.Count > 6)
        {
            warning.DisplayWarning(TEAM_MEMBER_ERROR_MESSAGE);
            return;
        }
        for (int i = 0; i < 10; i++)
        {
            if (TeamManager.instance.team.memberList.Count(x => x == i) > 3)
            {
                warning.DisplayWarning(TEAM_MEMBER_ERROR_MESSAGE);
                return;
            }
        }
        SceneManager.LoadScene("BattleScene");
    }

    public void EditButton()
    {
        SceneManager.LoadScene("Select");
    }
}
