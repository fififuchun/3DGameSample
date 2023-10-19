using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalManager : MonoBehaviour
{
    // public TeamManager teamManager = new TeamManager();
    public Sprite[] pinHeadImages = new Sprite[10];
    public Image[] pinsSprite = new Image[6];

    void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            // Debug.Log(TeamManager.instance.team.memberList[i]);
            pinsSprite[i].sprite= pinHeadImages[TeamManager.instance.team.memberList[i]];
        }
    }

    void Update()
    {

    }
}
