using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mergepins.Network
{
    public class RoomGUI : MonoBehaviour
    {
        public GameObject playerList;
        public GameObject playerPrefab;
        public GameObject cancelButton;
        public GameObject leaveButton;
        public GameObject copyButton;
        public Button startButton;
        public Text text;
        public bool owner;

        public void RefreshRoomPlayers(PlayerInfo[] playerInfos)
        {
            // Debug.Log($"RefreshRoomPlayers: {playerInfos.Length} playerInfos");

            foreach (Transform child in playerList.transform)
            {
                Destroy(child.gameObject);
            }

            startButton.interactable = false;

            foreach (PlayerInfo playerInfo in playerInfos)
            {
                GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                newPlayer.transform.SetParent(playerList.transform, false);
                newPlayer.GetComponent<PlayerGUI>().SetPlayerInfo(playerInfo);
            }

            startButton.interactable = owner && (playerInfos.Length > 1);
        }

        public void SetOwner(bool owner)
        {
            this.owner = owner;
            cancelButton.SetActive(owner);
            leaveButton.SetActive(!owner);
            copyButton.SetActive(owner);
        }

        public void ShowRoomID(Guid guid)
        {
            text.text = "Your room ID is \n" + guid.ToString();
        }
    }
}
