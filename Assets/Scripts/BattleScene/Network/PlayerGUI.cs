using UnityEngine;
using UnityEngine.UI;

namespace Mergepins.Network
{
    public class PlayerGUI : MonoBehaviour
    {
        public Text playerName;

        public void SetPlayerInfo(PlayerInfo info)
        {
            playerName.text = info.playerName;
        }
    }
}
