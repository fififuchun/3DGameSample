using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DG.Tweening;
using Mergepins;

public partial class Selection : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup teamCanvasGroup;
    private Transform teamCanvasTransform;
    [SerializeField]
    private CanvasGroup selectionCanvasGroup;
    private CanvasGroup buttonPanelCanvasGroup;
    private CanvasGroup editPanelCanvasGroup;
    [System.NonSerialized]
    public CanvasGroup scrollContentCanvasGroup;
    [SerializeField]
    private CanvasGroup setTeamCanvasGroup;
    private TeamLayout teamLayout;
    [SerializeField]
    private InputField inputField;
    [SerializeField]
    private Transform PinPanel;
    private PinLayout pinLayout;
    [SerializeField]
    private Transform scrollContentTransform;
    [SerializeField]
    private ScrollRectSnap snap;
    [SerializeField]
    public List<GameObject> pinImagePrefab;
    [SerializeField]
    private GameObject teamPrefab;
    [SerializeField]
    private GameObject element;
    [SerializeField]
    private WarningDisplayer displayer;

    private int teamIndex;

    private string datapath;
    private Team team;
    public TeamData teamData { get; private set; }
    // 初期化
    private void Awake()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/SaveData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SaveData");
        }
        this.datapath = Application.persistentDataPath + "/SaveData/team_data.json";
        this.team = new Team();
        this.teamData = new TeamData();
        this.teamCanvasTransform = this.teamCanvasGroup.transform.Find("Panel");
        this.buttonPanelCanvasGroup = this.selectionCanvasGroup.transform.Find("ButtonPanel").GetComponent<CanvasGroup>();
        this.editPanelCanvasGroup = this.selectionCanvasGroup.transform.Find("EditPanel").GetComponent<CanvasGroup>();
        this.scrollContentCanvasGroup = this.scrollContentTransform.GetComponent<CanvasGroup>();
        this.teamLayout = this.scrollContentTransform.GetComponent<TeamLayout>();
        this.pinLayout = this.PinPanel.GetComponent<PinLayout>();
    }

    void Start()
    {
        this.LoadTeam();
        this.ShowAllTeams();
    }

    private void AddNewTeam()
    {
        this.team.name = this.inputField.text;
        this.teamData.teamList.Insert(0, this.team);
        this.SaveTeam();
        Transform instance = Instantiate(teamPrefab, scrollContentTransform).transform;
        instance.SetSiblingIndex(0);
        instance.Find("NamePanel/Image/Text").GetComponent<Text>().text = this.team.name;
        Transform teamArea = instance.Find("TeamArea");
        for (int i = 0; i < this.team.memberList.Count; i++)
        {
            Instantiate(pinImagePrefab[this.team.memberList[i]], teamArea);
        }
        this.scrollContentTransform.parent.parent.GetChild(1).GetComponent<Scrollbar>().value = 1;
        this.teamLayout.SetPosition(instance, 0);
        this.teamLayout.Align(1);
        this.team = new Team();
        this.inputField.text = "";
        foreach (Transform child in this.PinPanel)
        {
            Destroy(child.gameObject);
        }
    }

    private void RebuildTeam()
    {
        this.team.name = this.inputField.text;
        this.teamData.teamList.RemoveAt(this.teamIndex);
        this.teamData.teamList.Insert(this.teamIndex, this.team);
        this.SaveTeam();
        Transform team = this.scrollContentTransform.GetChild(this.teamIndex);
        team.Find("NamePanel/Image/Text").GetComponent<Text>().text = this.team.name;
        Transform teamArea = team.Find("TeamArea");
        foreach (Transform child in teamArea)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < this.team.memberList.Count; i++)
        {
            Instantiate(pinImagePrefab[this.team.memberList[i]], teamArea);
        }
        this.team = new Team();
        this.inputField.text = "";
        foreach (Transform child in this.PinPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddPin(int number)
    {
        int count = this.team.memberList.Count;
        if (count >= 6)
        {
            this.displayer.DisplayWarning("6体以上ピンを追加できません");
            return;
        }
        if (this.team.memberList.Count(x => x == number) > 2)
        {
            this.displayer.DisplayWarning("同じピンを4体以上追加できません");
            return;
        }
        this.team.memberList.Add(number);
        Transform instance = Instantiate(pinImagePrefab[number], PinPanel).transform;
        this.pinLayout.SetPosition(instance, count);
    }

    public void RemoveTeam(int index)
    {
        this.teamData.teamList.RemoveAt(index);
        this.SaveTeam();
    }

    public void RemovePin(int index)
    {
        this.team.memberList.RemoveAt(index);
    }

    public void ReplaceTeams(int removeIndex, int insertIndex)
    {
        if (removeIndex > insertIndex)
        {
            this.teamData.teamList.Insert(insertIndex, this.teamData.teamList[removeIndex]);
            this.teamData.teamList.RemoveAt(removeIndex + 1);
            this.SaveTeam();
        }
        else if (removeIndex < insertIndex)
        {
            this.teamData.teamList.Insert(insertIndex + 1, this.teamData.teamList[removeIndex]);
            this.teamData.teamList.RemoveAt(removeIndex);
            this.SaveTeam();
        }
    }

    public void ReplacePins(int removeIndex, int insertIndex)
    {
        if (removeIndex > insertIndex)
        {
            this.team.memberList.Insert(insertIndex, this.team.memberList[removeIndex]);
            this.team.memberList.RemoveAt(removeIndex + 1);
        }
        else if (removeIndex < insertIndex)
        {
            this.team.memberList.Insert(insertIndex + 1, this.team.memberList[removeIndex]);
            this.team.memberList.RemoveAt(removeIndex);
        }
    }

    private void SaveTeam()
    {
        string jsondata = JsonUtility.ToJson(this.teamData);
        File.WriteAllText(this.datapath, jsondata);
    }

    private void LoadTeam()
    {
        if (File.Exists(this.datapath))
        {
            string jsondata = File.ReadAllText(this.datapath);
            if (jsondata != "")
            {
                this.teamData = JsonUtility.FromJson<TeamData>(jsondata);
            }
        }
    }

    private void ShowAllTeams()
    {
        for (int i = 0; i < this.teamData.teamList.Count; i++)
        {
            Transform instance = Instantiate(teamPrefab, scrollContentTransform).transform;
            instance.Find("NamePanel/Image/Text").GetComponent<Text>().text = this.teamData.teamList[i].name;
            Transform teamArea = instance.Find("TeamArea");
            for (int j = 0; j < this.teamData.teamList[i].memberList.Count; j++)
            {
                Instantiate(pinImagePrefab[this.teamData.teamList[i].memberList[j]], teamArea);
            }
            this.teamLayout.SetPosition(instance, i);
        }
        this.teamLayout.FitSize();
    }

    public void CreateTeamButton(bool create)
    {
        if (create)
        {
            this.teamCanvasGroup.Enable(1.0f);
            this.teamCanvasTransform.DOScale(1.0f, 0.24f).SetEase(Ease.OutCubic);
            this.selectionCanvasGroup.Disable(0.2f);
            this.teamIndex = -1;
        }
        else
        {
            this.teamCanvasGroup.Disable(0.0f);
            this.teamCanvasTransform.DOScale(0.0f, 0.24f).SetEase(Ease.InCubic);
            this.selectionCanvasGroup.Enable(1.0f);
            this.team = new Team();
            this.inputField.text = "";
            foreach (Transform child in this.PinPanel)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void SaveTeamButton()
    {
        if (this.inputField.text == "")
        {
            this.displayer.DisplayWarning("名前を入力してください。");
        }
        else if (this.PinPanel.childCount == 0)
        {
            this.displayer.DisplayWarning("ピンを追加してください。");
        }
        else
        {
            if (this.teamIndex == -1)
            {
                this.AddNewTeam();
            }
            else if (this.teamIndex > -1)
            {
                this.RebuildTeam();
            }
            this.teamCanvasGroup.Disable(0.0f);
            this.teamCanvasTransform.DOScale(0.0f, 0.24f).SetEase(Ease.InCubic);
            this.selectionCanvasGroup.Enable(1.0f);
        }
    }

    public void RebuildTeamButton(int afterIndex)
    {
        this.teamCanvasGroup.Enable(1.0f);
        this.teamCanvasTransform.DOScale(1.0f, 0.24f).SetEase(Ease.OutCubic);
        this.selectionCanvasGroup.Disable(0.2f);
        this.team = this.teamData.teamList[afterIndex];
        this.inputField.text = this.team.name;
        for (int i = 0; i < this.team.memberList.Count; i++)
        {
            Transform instance = Instantiate(pinImagePrefab[this.team.memberList[i]], PinPanel).transform;
            this.pinLayout.SetPosition(instance, i);
        }
        this.teamIndex = afterIndex;
    }

    public void SetEditMode(bool edit)
    {
        if (edit)
        {
            this.scrollContentCanvasGroup.Enable();
            this.buttonPanelCanvasGroup.Disable(0.3f);
            this.editPanelCanvasGroup.Enable(1.0f);
        }
        else
        {
            this.scrollContentCanvasGroup.Disable();
            this.buttonPanelCanvasGroup.Enable(1.0f);
            this.editPanelCanvasGroup.Disable(0.0f);
        }
    }

    public bool SetTeamButton()
    {
        if (this.teamData.teamList.Count == 0)
        {
            this.displayer.DisplayWarning("一つ以上チームを作成してください");
            return false;
        }
        this.selectionCanvasGroup.Disable(0.0f);
        this.setTeamCanvasGroup.Enable(1.0f);
        this.ShowAllName();
        return true;
    }

    public void EscapeSetTeamButton()
    {
        this.selectionCanvasGroup.Enable(1.0f);
        this.setTeamCanvasGroup.Disable(0.0f);
    }

    public bool SetSelected(int index)
    {
        if (this.teamData.teamList[index].memberList.Count < 6)
        {
            this.displayer.DisplayWarning("チームのピンが6体に満たないため、\n選択できません。");
            return false;
        }
        this.teamData.selected = index;
        this.SaveTeam();
        return true;
    }

    public void ShowAllName()
    {
        for (int i = 0; i < this.teamData.teamList.Count; i++)
        {
            Transform instance = Instantiate(this.element, this.snap.content).transform;
            instance.GetChild(0).GetComponent<Text>().text = this.teamData.teamList[i].name;
        }
        this.snap.Init();
    }
}

[System.Serializable]
public class TeamData
{
    public int selected = 0;
    public List<Team> teamList = new List<Team>();
}

[System.Serializable]
public class Team
{
    public string name;
    public List<int> memberList = new List<int>();
}