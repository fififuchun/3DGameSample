using System;
using Mirror;

namespace Mergepins.Network
{
    /// <summary>
    /// Match message to be sent to the server
    /// </summary>
    public struct ServerMatchMessage : NetworkMessage
    {
        public ServerMatchOperation serverMatchOperation;
        public Guid matchId;
    }

    public struct LocalPlayerInfoToServer : NetworkMessage
    {
        public string playerName;
    }

    /// <summary>
    /// Match message to be sent to the client
    /// </summary>
    public struct ClientMatchMessage : NetworkMessage
    {
        public ClientMatchOperation clientMatchOperation;
        public Guid matchId;
        public MatchInfo[] matchInfos;
        public PlayerInfo[] playerInfos;
    }

    /// <summary>
    /// Information about a match
    /// </summary>
    [Serializable]
    public struct MatchInfo
    {
        public Guid matchId;
        public byte players;
        public byte maxPlayers;
    }

    /// <summary>
    /// Information about a player
    /// </summary>
    [Serializable]
    public struct PlayerInfo
    {
        public string playerName;
        public Guid matchId;
    }

    public struct PartyInfo : NetworkMessage
    {
        public string playerName;
        public string iconURL;
        public int[] party;
    }

    /// <summary>
    /// Information about actions that a player chose on a match
    /// </summary>
    [Serializable]
    public struct MatchPlayerAction
    {
        public PlayerAction playerAction;
        public PlayerAction woodAction;
        public int[] newly_merged_pins;
        public int change_index;
        public int storm_elem_action_index;
        public int norm_action_type;
        public int regenerate_selected_index;
    }

    /// <summary>
    /// Match operation to execute on the server
    /// </summary>
    public enum ServerMatchOperation : byte
    {
        None,
        Create,
        Cancel,
        Start,
        Join,
        Leave
    }

    /// <summary>
    /// Match operation to execute on the client
    /// </summary>
    public enum ClientMatchOperation : byte
    {
        None,
        List,
        Created,
        Cancelled,
        Joined,
        Departed,
        UpdateRoom,
        Started
    }
}
