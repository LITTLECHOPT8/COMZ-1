using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunCallback : Photon.PunBehaviour
{
	public static PunCallback CreateCallback()
	{
		GameObject obj = GameObject.Find("callbacks");
		if (obj == null)
		{
			obj = new GameObject("callbacks");
		}
		PunCallback callback = obj.AddComponent<PunCallback>();
		DontDestroyOnLoad(callback.gameObject);
		return callback;
	}

	public delegate void Connect();

	public delegate void Disconnect();

	public delegate void JoinRoom();

	public delegate void LeftRoom();

	public delegate void ConnectedToMaster();

	public delegate void PlayerJoined(PhotonPlayer connectedPlayer);

	public delegate void PlayerLeft(PhotonPlayer disconnectedPlayer);

	public delegate void PlayerPropertiesChanged(object[] playerAndUpdatedProps);

	public delegate void RoomPropertiesChanged(ExitGames.Client.Photon.Hashtable changedProps);

	public delegate void ServerEvent(ServerEventData data);

	public Connect connect;

	public Disconnect disconnect;

	public JoinRoom join_room;

	public LeftRoom left_room;

	public ConnectedToMaster connected_to_master;

	public PlayerJoined player_joined;

	public PlayerLeft player_left;

	public PlayerPropertiesChanged player_props_changed;

	public RoomPropertiesChanged room_props_changed;

	public ServerEvent server_event;

	private void Start()
	{
		PhotonNetwork.OnEventCall += OnEvent;
	}

	public void OnEvent(byte eventCode, object content, int senderId)
	{
		OnServerEvent(new ServerEventData { eventCode = eventCode, data = (object[])content, sender = FindPlayer(senderId) } );
	}

	private PhotonPlayer FindPlayer(int ID)
	{
		foreach (PhotonPlayer player in PhotonNetwork.playerList)
		{
			if (player.ID == ID)
			{
				return player;
			}
		}
		return null;
	}

	public void OnServerEvent(ServerEventData data)
	{
		if (server_event != null)
		{
			server_event(data);
		}
	}

    public override void OnJoinedLobby()
    {
        if (connect != null)
		{
			connect();
		}
    }

	public override void OnDisconnectedFromPhoton()
	{
		if (disconnect != null)
		{
			disconnect();
		}
	}

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
		if (player_props_changed != null)
		{
			player_props_changed(playerAndUpdatedProps);
		}
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
		if (player_joined != null)
		{
			player_joined(newPlayer);
		}
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
		if (player_left != null)
		{
			player_left(otherPlayer);
		}
    }

    public override void OnLeftRoom()
    {
		if (left_room != null)
		{
			left_room();
		}
    }

    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (room_props_changed != null)
		{
			room_props_changed(propertiesThatChanged);
		}
    }

    public override void OnConnectedToMaster()
    {
		if (connected_to_master != null)
		{
			connected_to_master();
		}
    }

    public override void OnJoinedRoom()
    {
		if (join_room != null)
		{
			join_room();
		}
    }
}
