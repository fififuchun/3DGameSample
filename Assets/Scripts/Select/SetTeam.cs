using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Mergepins;

public class SetTeam : MonoBehaviour
{
    [SerializeField]
    private Selection selection;
    [SerializeField]
    private ScrollRectSnap snap;
    [SerializeField]
    private CustomButton setTeamButton;
    [SerializeField]
    private CustomButton escapeButton;
    [SerializeField]
    private CustomButton setSelectedButton;
    [SerializeField]
    private CanvasGroup buttonCanvasGroup;

    private void Awake()
    {
        this.setTeamButton.OnClick.AddListener(() => { if (this.selection.SetTeamButton()) this.PinEnter(); });
        this.escapeButton.OnClick.AddListener(EscapeButoon);

        this.setSelectedButton.OnClick.AddListener(() => { if (this.selection.SetSelected(this.snap.index)) this.BackToPortal(); });

        this.snap.BeginSetIndex += () => this.buttonCanvasGroup.Disable(0.3f);
        this.snap.BeginSetIndex += PinExit;

        this.snap.CompleteSetIndex += () => this.buttonCanvasGroup.Enable(1.0f);
        this.snap.CompleteSetIndex += PinEnter;
    }

    private void BackToPortal()
    {
        SceneManager.LoadScene("Portal");
    }

    private List<Transform> instances = new List<Transform>();

    private void PinEnter()
    {
        Team team = this.selection.teamData.teamList[this.snap.index];
        for (int i = 0; i < team.memberList.Count; i++)
        {
            Transform instance = Instantiate(this.selection.pinImagePrefab[team.memberList[i]], this.snap.content).transform;
            this.instances.Add(instance);
            instance.SetSiblingIndex(0);
            this.snap.SetPosition(instance, this.snap.index);
            ((RectTransform)instance).DOAnchorPosX(-1250 + 160 * i, 0.4f + 0.1f * (i + 1)).SetRelative().SetEase(Ease.OutCubic).SetLink(instance.gameObject);
        }
    }

    private void PinExit()
    {
        for (int i = 0; i < this.instances.Count; i++)
        {
            this.instances[i].DOKill();
            Destroy(this.instances[i].gameObject);
        }
        this.instances.Clear();
    }

    private void EscapeButoon()
    {
        this.PinExit();
        foreach (Transform child in this.snap.content)
        {
            Destroy(child.gameObject);
        }
    }
}
