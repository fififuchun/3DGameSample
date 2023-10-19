using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Mergepins.Network
{
    [RequireComponent(typeof(NetworkMatch))]
    public class MatchController : NetworkBehaviour
    {
        [Header("Diagnostics - Do Not Modify")]
        public CanvasController canvasController;
        public GameObject objectsUsedInGame;
        private TopIndexSelector topIndexSelector;

        public NetworkIdentity player1;
        public NetworkIdentity player2;

        private PartyInfo partyInfo1 = new PartyInfo { party = null };
        private PartyInfo partyInfo2 = new PartyInfo { party = null };

        private int topIndex1 = -1;
        private int topIndex2 = -1;

        private MatchPlayerAction action1;
        private MatchPlayerAction action2;

        public static MatchController instance;
        void Awake()
        {
            instance = this;
            canvasController = FindObjectOfType<CanvasController>();
        }

        public override void OnStartServer()
        {
        }

        public override void OnStartClient()
        {
            objectsUsedInGame = canvasController.transform.Find("ObjectsUsedInGame").gameObject;
            canvasController.transform.Find("TurnManager").gameObject.SetActive(true);
            topIndexSelector = canvasController.transform.Find("TopIndexSelector").GetComponent<TopIndexSelector>();
            CmdRegisterParty(new PartyInfo
            {
                party = TeamManager.instance.team.memberList.ToArray(),
                playerName = TeamManager.instance.userData.userName,
                iconURL = TeamManager.instance.userData.iconURL
            });
        }

        [Command(requiresAuthority = false)]
        public void CmdRegisterParty(PartyInfo partyInfo, NetworkConnectionToClient sender = null)
        {
            if (sender == player1.connectionToClient)
                partyInfo1 = partyInfo;
            else if (sender == player2.connectionToClient)
                partyInfo2 = partyInfo;
            if (partyInfo1.party == null || partyInfo2.party == null) return;
            RpcInformParty(player1, player2, partyInfo1, partyInfo2);
        }

        [ClientRpc]
        public void RpcInformParty(NetworkIdentity player1, NetworkIdentity player2, PartyInfo partyInfo1, PartyInfo partyInfo2)
        {
            this.player1 = player1;
            this.player2 = player2;
            if (player1.isLocalPlayer)
            {
                this.partyInfo2 = partyInfo2;
                Texture2D texture2D = new Texture2D(1, 1);

                TurnManager.instance.my.party = partyInfo1.party;
                topIndexSelector.myNameText.text = partyInfo1.playerName;
                StartCoroutine(TextureDownloader.DownloadTexture(partyInfo1.iconURL, t => topIndexSelector.myIconImage.texture = t));

                TurnManager.instance.enemy.party = partyInfo2.party;
                topIndexSelector.enemyNameText.text = partyInfo2.playerName;
                StartCoroutine(TextureDownloader.DownloadTexture(partyInfo2.iconURL, t => topIndexSelector.enemyIconImage.texture = t));
            }
            else if (player2.isLocalPlayer)
            {
                this.partyInfo1 = partyInfo1;
                Texture2D texture2D = new Texture2D(1, 1);

                TurnManager.instance.my.party = partyInfo2.party;
                topIndexSelector.myNameText.text = partyInfo2.playerName;
                StartCoroutine(TextureDownloader.DownloadTexture(partyInfo2.iconURL, t => topIndexSelector.myIconImage.texture = t));

                TurnManager.instance.enemy.party = partyInfo1.party;
                topIndexSelector.enemyNameText.text = partyInfo1.playerName;
                StartCoroutine(TextureDownloader.DownloadTexture(partyInfo1.iconURL, t => topIndexSelector.enemyIconImage.texture = t));
            }
            topIndexSelector.gameObject.SetActive(true);
        }

        [Command(requiresAuthority = false)]
        public void CmdRegisterTopIndex(int index, NetworkConnectionToClient sender = null)
        {
            if (sender == player1.connectionToClient)
                topIndex1 = index;
            else if (sender == player2.connectionToClient)
                topIndex2 = index;
            if (topIndex1 == -1 || topIndex2 == -1) return;
            RpcStartGame(topIndex1, topIndex2);
        }

        [ClientRpc]
        public void RpcStartGame(int topIndex1, int topIndex2)
        {
            this.topIndex1 = topIndex1;
            this.topIndex2 = topIndex2;
            if (player1.isLocalPlayer)
            {
                TurnManager.instance.my.topIndex = topIndex1;
                TurnManager.instance.enemy.topIndex = topIndex2;
            }
            else if (player2.isLocalPlayer)
            {
                TurnManager.instance.my.topIndex = topIndex2;
                TurnManager.instance.enemy.topIndex = topIndex1;
            }
            topIndexSelector.gameObject.SetActive(false);
            objectsUsedInGame.SetActive(true);
            TurnManager.instance.StartGame();
        }

        [Command(requiresAuthority = false)]
        public void CmdRegisterPlayerAction(MatchPlayerAction action, NetworkConnectionToClient sender = null)
        {
            if (sender == player1.connectionToClient)
                action1 = action;
            else if (sender == player2.connectionToClient)
                action2 = action;

            if (action1.playerAction == PlayerAction.None || action2.playerAction == PlayerAction.None) return;
            RpcInformPlayerAction(action1, action2, Random.Range(0, 2) == 0);
        }

        [ClientRpc]
        public void RpcInformPlayerAction(MatchPlayerAction action1, MatchPlayerAction action2, bool random)
        {
            this.action1 = action1;
            this.action2 = action2;
            if (player1.isLocalPlayer)
            {
                TurnManager.instance.my.InformedPlayerActions(action1);
                TurnManager.instance.enemy.InformedPlayerActions(action2);
                TurnManager.instance.GoFromWaitToNext(random);
            }
            else if (player2.isLocalPlayer)
            {
                TurnManager.instance.my.InformedPlayerActions(action2);
                TurnManager.instance.enemy.InformedPlayerActions(action1);
                TurnManager.instance.GoFromWaitToNext(!random);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdLoopEnded(NetworkConnectionToClient sender = null)
        {
            if (sender == player1.connectionToClient)
                action1.playerAction = PlayerAction.None;
            else if (sender == player2.connectionToClient)
                action2.playerAction = PlayerAction.None;

            if (action1.playerAction == PlayerAction.None && action2.playerAction == PlayerAction.None)
                RpcBackToWait();
        }

        [ClientRpc]
        public void RpcBackToWait()
        {
            TurnManager.instance.BackToWait();
        }

        // Assigned in inspector to BackButton::OnClick
        [Client]
        public void RequestExitGame()
        {
            // exitButton.gameObject.SetActive(false);
            // playAgainButton.gameObject.SetActive(false);
            CmdRequestExitGame();
        }

        [Command(requiresAuthority = false)]
        public void CmdRequestExitGame(NetworkConnectionToClient sender = null)
        {
            StartCoroutine(ServerEndMatch(sender, false));
        }

        public void OnPlayerDisconnected(NetworkConnectionToClient conn)
        {
            // Check that the disconnecting client is a player in this match
            if (player1 == conn.identity || player2 == conn.identity)
            {
                StartCoroutine(ServerEndMatch(conn, true));
            }
        }

        public IEnumerator ServerEndMatch(NetworkConnectionToClient conn, bool disconnected)
        {
            canvasController.OnPlayerDisconnected -= OnPlayerDisconnected;

            RpcExitGame();

            // Skip a frame so the message goes out ahead of object destruction
            yield return null;

            // Mirror will clean up the disconnecting client so we only need to clean up the other remaining client.
            // If both players are just returning to the Lobby, we need to remove both connection Players

            if (!disconnected)
            {
                NetworkServer.RemovePlayerForConnection(player1.connectionToClient, true);
                CanvasController.waitingConnections.Add(player1.connectionToClient);

                NetworkServer.RemovePlayerForConnection(player2.connectionToClient, true);
                CanvasController.waitingConnections.Add(player2.connectionToClient);
            }
            else if (conn == player1.connectionToClient)
            {
                // player1 has disconnected - send player2 back to Lobby
                NetworkServer.RemovePlayerForConnection(player2.connectionToClient, true);
                CanvasController.waitingConnections.Add(player2.connectionToClient);
            }
            else if (conn == player2.connectionToClient)
            {
                // player2 has disconnected - send player1 back to Lobby
                NetworkServer.RemovePlayerForConnection(player1.connectionToClient, true);
                CanvasController.waitingConnections.Add(player1.connectionToClient);
            }

            // Skip a frame to allow the Removal(s) to complete
            yield return null;

            // Send latest match list
            canvasController.SendMatchList();

            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        public void RpcExitGame()
        {
            canvasController.OnMatchEnded();
        }

        private void OnDestroy()
        {
            TurnManager.instance.EndGame();
            objectsUsedInGame.SetActive(false);
            topIndexSelector.gameObject.SetActive(false);
        }
    }
}
