using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class UserData
{
    public UserData(string userName)
    {
        this.userName = userName;
    }

    public string userName;
    public string iconURL;
}

public class TeamManager : MonoBehaviour
{
    static public TeamManager instance;

    [System.NonSerialized] public Team team = new Team();
    [System.NonSerialized] public Texture2D iconTexture = null;
    [System.NonSerialized] public UserData userData = null;

    private string SaveDirectoryPath;
    private string TeamDataFilePath;
    private string IconDataFilePath;
    private string UserDataFilePath;

    private string _imgurClientID = "fc7c4417f695908";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SaveDirectoryPath = Application.persistentDataPath + "/SaveData";
            TeamDataFilePath = SaveDirectoryPath + "/team_data.json";
            IconDataFilePath = SaveDirectoryPath + "/icon.png";
            UserDataFilePath = SaveDirectoryPath + "/user_data.json";
            if (!Directory.Exists(SaveDirectoryPath)) Directory.CreateDirectory(SaveDirectoryPath);
            LoadSelectedTeam();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSelectedTeam()
    {
        if (File.Exists(TeamDataFilePath))
        {
            string jsondata = File.ReadAllText(TeamDataFilePath);
            if (jsondata != "")
            {
                TeamData teamData = JsonUtility.FromJson<TeamData>(jsondata);
                this.team = teamData.teamList[teamData.selected];
            }
        }
    }

    public void LoadUserIcon()
    {
        if (File.Exists(IconDataFilePath))
        {
            byte[] bytes = File.ReadAllBytes(IconDataFilePath);
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(bytes);
            iconTexture = texture2D;
        }
    }

    public void LoadUserData()
    {
        if (File.Exists(UserDataFilePath))
        {
            string jsondata = File.ReadAllText(UserDataFilePath);
            if (jsondata != "")
            {
                userData = JsonUtility.FromJson<UserData>(jsondata);
            }
        }
    }

    public void SaveUserIcon()
    {
        File.WriteAllBytes(IconDataFilePath, iconTexture.EncodeToPNG());
        StartCoroutine(ImgurUploader.UploadToImgur(_imgurClientID, iconTexture, OnUploadSuccess, s => Debug.Log(s)));
    }

    private void OnUploadSuccess(string link)
    {
        userData.iconURL = link;
        SaveUserData();
    }

    public void SaveUserData()
    {
        File.WriteAllText(UserDataFilePath, JsonUtility.ToJson(userData));
    }
}