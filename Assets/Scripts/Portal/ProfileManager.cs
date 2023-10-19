using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Kakera;
using Mergepins;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] private Texture2D[] DefaultUserIcons;
    [SerializeField] private string DefaultUserName;

    [SerializeField] private RawImage iconImage;
    [SerializeField] private TextMeshProUGUI userNameText;

    [SerializeField] private CanvasGroup editCanvasGroup;
    [SerializeField] private CanvasGroup backCanvasGroup;
    [SerializeField] private RawImage editIconImage;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI badNameText;
    private int minNameLength = 3;
    private int maxNameLength = 10;

    [SerializeField] private CanvasGroup DefaultCanvasGroup;

    [SerializeField] private Unimgpicker imagePicker;
    private int iconSize = 256;
    bool imageLoaded = false;

    private void Awake()
    {
        imagePicker.Completed += path => LoadImage(path);
        nameInputField.onValueChanged.AddListener(OnInputFieldChanged);
        nameInputField.onSubmit.AddListener(OnInputFieldChanged);
        nameInputField.onEndEdit.AddListener(OnInputFieldChanged);
    }

    private void Start()
    {
        TeamManager.instance.LoadUserIcon();
        if (TeamManager.instance.iconTexture == null)
        {
            TeamManager.instance.iconTexture = getCenterClippedTexture(DefaultUserIcons[Random.Range(0, DefaultUserIcons.Length)]);
            TextureScale.Bilinear(TeamManager.instance.iconTexture, iconSize, iconSize);
            TeamManager.instance.SaveUserIcon();
        }

        TeamManager.instance.LoadUserData();
        if (TeamManager.instance.userData == null)
        {
            TeamManager.instance.userData = new UserData(DefaultUserName);
            TeamManager.instance.SaveUserData();
        }

        iconImage.texture = TeamManager.instance.iconTexture;
        userNameText.text = TeamManager.instance.userData.userName;
    }

    public void OnPressShowPicker()
    {
        imagePicker.Show("Select Image", "unimgpicker", 512);
    }

    public void OpenProfileEditor()
    {
        editIconImage.texture = iconImage.texture;
        nameInputField.text = userNameText.text;

        editCanvasGroup.Enable(1.0f);
        backCanvasGroup.Enable();
    }

    public void CloseProfileEditor()
    {
        string newName = nameInputField.text;
        if (IsBadName(newName)) return;

        if (imageLoaded)
        {
            imageLoaded = false;
            iconImage.texture = editIconImage.texture;
            TeamManager.instance.iconTexture = (Texture2D)editIconImage.texture;
            TeamManager.instance.SaveUserIcon();
        }

        userNameText.text = newName;
        TeamManager.instance.userData.userName = newName;
        TeamManager.instance.SaveUserData();

        editCanvasGroup.Disable(0.0f);
        backCanvasGroup.Disable();
        CloseDefaultPanel();
    }

    private bool IsBadName(string name)
    {
        return !(minNameLength <= name.Length && name.Length <= maxNameLength);
    }

    public void OnInputFieldChanged(string value)
    {
        badNameText.enabled = IsBadName(value);
    }

    public void OpenDefaultPanel()
    {
        DefaultCanvasGroup.Enable(1);
    }

    public void CloseDefaultPanel()
    {
        DefaultCanvasGroup.Disable(0);
    }

    public void ChooseDefault(int index)
    {
        Texture2D texture2D = getCenterClippedTexture(DefaultUserIcons[index]);
        TextureScale.Bilinear(texture2D, iconSize, iconSize);
        editIconImage.texture = texture2D;
        imageLoaded = true;
    }

    private void LoadImage(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture2D = new Texture2D(1, 1);
        texture2D.LoadImage(bytes);
        texture2D = getCenterClippedTexture(texture2D);
        TextureScale.Bilinear(texture2D, iconSize, iconSize);
        editIconImage.texture = texture2D;
        imageLoaded = true;
    }

    private Texture2D getCenterClippedTexture(Texture2D texture)
    {
        Color[] pixel;
        Texture2D clipTex;
        int textureWidth = texture.width;
        int textureHeight = texture.height;

        if (textureWidth == textureHeight)
            return texture;
        if (textureWidth > textureHeight)
        { // 横の方が長い
            int x = (textureWidth - textureHeight) / 2;
            // GetPixels (x, y, width, height) で切り出せる
            pixel = texture.GetPixels(x, 0, textureHeight, textureHeight);
            // 横幅，縦幅を指定してTexture2Dを生成
            clipTex = new Texture2D(textureHeight, textureHeight);
        }
        else
        { // 縦の方が長い
            int y = (textureHeight - textureWidth) / 2;
            pixel = texture.GetPixels(0, y, textureWidth, textureWidth);
            clipTex = new Texture2D(textureWidth, textureWidth);
        }
        clipTex.SetPixels(pixel);
        clipTex.Apply();
        return clipTex;
    }
}
