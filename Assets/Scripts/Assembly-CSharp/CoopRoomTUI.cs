using UnityEngine;
using Zombie3D;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TNetSdk;

public class CoopRoomTUI : MonoBehaviour, TUIHandler
{
	private TUI m_tui;

	private TUIInput[] input;

	private AudioPlayer audioPlayer = new AudioPlayer();

	public GameObject MsgBox;

	public GameObject IndicatorPanel;

	public GameObject StartButton;

	public CoopRoomOwnerPanel CellPanel;

	public TUIMeshText RoomID;

	public TUIMeshSprite coopBK;

	private TNetObject tnetObj;

	private void Awake()
	{
		if (GameApp.GetInstance().GetGameState().gameMode != GameMode.Coop)
		{
			base.enabled = false;
			return;
		}
		base.enabled = true;
		GameScript.CheckFillBlack();
		GameScript.CheckMenuResourceConfig();
		GameScript.CheckGlobalResourceConfig();
		//NetworkObj.CheckNetWorkCom();
		if (GameObject.Find("Music") == null)
		{
			GameApp.GetInstance().InitForMenu();
			GameObject gameObject = new GameObject("Music");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.transform.position = new Vector3(0f, 1f, -10f);
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.clip = GameApp.GetInstance().GetMenuResourceConfig().menuAudio;
			audioSource.loop = true;
			audioSource.bypassEffects = true;
			audioSource.rolloffMode = AudioRolloffMode.Linear;
			audioSource.mute = !GameApp.GetInstance().GetGameState().MusicOn;
			audioSource.Play();
		}
		tnetObj = TNetConnection.Connection;
		tnetObj.SetOnRoomPropsChanged(OnRoomPropsUpdated);
		tnetObj.SetOnPlayerPropsChanged(OnPlayerPropsChanged);
		tnetObj.SetOnLeftRoom(OnLeaveRoom);
		tnetObj.SetOnPlayerLeft(PlayerLeft);
		Hashtable table = new Hashtable();
		table.Add("nickname", GameApp.GetInstance().GetGameState().nick_name);
		table.Add("avatarType", (int)GameApp.GetInstance().GetGameState().Avatar);
		table.Add("avatarLevel", GameApp.GetInstance().GetGameState().LevelNum);
		table.Add("weaponNum", 0);
		table.Add("playerState", 0);
		table.Add("playerBonusState", 0);
		table.Add("weapon1", 0);
		table.Add("weapon2", 0);
		table.Add("weapon3", 0);
		table.Add("weaponPara1", 0f);
		table.Add("weaponPara2", 0f);
		table.Add("weaponPara3", 0f);
		table.Add("birthPoint", 0);
		table.Add("spawnedIn", false);
		PhotonNetwork.player.SetCustomProperties(table);
		//net_com = GameApp.GetInstance().GetGameState().net_com;
		//net_com.packet_delegate = OnPacket;
		//net_com.close_delegate = OnContectingLost;
		//net_com.leave_room_delegate = OnLeaveRoom;
		//net_com.leave_room_notity_delegate = OnLeaveRoomNotify;
		//net_com.join_room_notity_delegate = OnJoinRoomNotify;
		//net_com.kick_player_delegate = OnKickPlayer;
		//net_com.be_kicked_delegate = OnBeKicked;
		//net_com.kick_player_notify_delegate = OnKickPlayerNotify;
		//net_com.destroy_delegate = OnDestroyRoom;
		//net_com.destroy_notify_delegate = OnDestroyRoomNotify;
		//net_com.contecting_lost = OnContectingLost;
		//net_com.contecting_timeout = OnContectingTimeout;
		//net_com.reverse_heart_timeout_delegate = OnReverseHearTimeout;
		//net_com.reverse_heart_renew_delegate = OnReverseHearRenew;
		//net_com.reverse_heart_waiting_delegate = OnReverseHearWaiting;
	}

	private void PlayerLeft(PhotonPlayer player)
	{
		if (PhotonNetwork.isMasterClient)
		{
			StartButton.SetActive(true);
		}
		else
		{
			StartButton.SetActive(false);
		}
		CellPanel.RefrashClientCellShow();
	}

	private void OnPlayerPropsChanged(object[] playerAndProps)
	{
		CellPanel.RefrashClientCellShow();
	}

	private void Start()
	{
		m_tui = TUI.Instance("TUI");
		m_tui.SetHandler(this);
		m_tui.transform.Find("TUIControl").Find("Fade").GetComponent<TUIFade>()
			.FadeIn();
		audioPlayer.AddAudio(base.transform, "Button", true);
		audioPlayer.AddAudio(base.transform, "Back", true);
		IndicatorPanel.GetComponent<IndicatorTUI>().automaticClose = OnContectingTimeout;
		CellPanel.RefrashClientCellShow();
		if (PhotonNetwork.isMasterClient)
		{
			StartButton.SetActive(true);
		}
		else
		{
			StartButton.SetActive(false);
		}
		GameObject.Find("TUI/TUIControl/MapName").SetActive(false);
		RoomID.text_Accessor = "ROOM " + PhotonNetwork.room.name.Split('|')[0];
		coopBK.gameObject.SetActive(true);
		coopBK.frameName_Accessor = GameApp.GetInstance().GetGameState().cur_net_map;
		OpenClikPlugin.Hide();
		Resources.UnloadUnusedAssets();
	}

	private void Update()
	{
		input = TUIInputManager.GetInput();
		for (int i = 0; i < input.Length; i++)
		{
			m_tui.HandleInput(input[i]);
		}
	}

	private void OnRoomPropsUpdated(Hashtable table)
	{
		if ((bool)table["gameStarted"])
		{
			GameApp.GetInstance().GetGameState().gameMode = GameMode.Coop;
			string cur_net_map = GameApp.GetInstance().GetGameState().cur_net_map;
			if (cur_net_map.Length > 0)
			{
				SceneName.FadeOutLevel(cur_net_map);
			}
			Debug.Log("Start Game Now...");
		}
	}

	private void OnLeaveRoom()
	{
		if (willingly)
		{
			SceneName.FadeOutLevel("CoopHallTUI");
		}
		else if (MsgBox != null)
		{
			if (!PhotonNetwork.connected)
			{
				MsgBox.GetComponent<MsgBoxDelegate>().Show("DISCONNECTED.", MsgBoxType.ContectingLost);
			}
			else
			{
				MsgBox.GetComponent<MsgBoxDelegate>().Show("YOU WERE REMOVED FROM \nTHE ROOM.", MsgBoxType.BeKicked);
			}
		}
	}

	private bool willingly;

	public void HandleEvent(TUIControl control, int eventType, float wparam, float lparam, object data)
	{
		if (control.name == "Back_Button" && eventType == 3)
		{
			audioPlayer.PlayAudio("Back");
			willingly = true;
			PhotonNetwork.LeaveRoom();
		}
		else if (control.name == "Start_Button" && eventType == 3)
		{
			audioPlayer.PlayAudio("Button");
			Hashtable table = PhotonNetwork.room.customProperties;
			table.Add("gameStarted", true);
			PhotonNetwork.room.SetCustomProperties(table);

			//if ((bool)CellPanel)
			//{
			//	if (CellPanel.roomUserCount > 0)
			//	{
			//		Debug.Log("Ready Start Game");
			//		net_com.Send(CGStartGamePacket.MakePacket());
			//	}
			//	else if (MsgBox != null)
			//	{
			//		MsgBox.GetComponent<MsgBoxDelegate>().Show("You need at least 1 other \nplayer to start the game.".ToUpper(), MsgBoxType.NotEnoughUser);
			//	}
			//}
		}
		else if (control.name == "kick" && eventType == 3)
		{
			audioPlayer.PlayAudio("Button");
			PhotonNetwork.CloseConnection(control.transform.parent.GetComponent<RoomCellData>().net_user);
		}
		else if (control.name == "Msg_OK_Button" && eventType == 3)
		{
			audioPlayer.PlayAudio("Button");
			MsgBoxType type = MsgBox.GetComponent<MsgBoxDelegate>().m_type;
			MsgBox.GetComponent<MsgBoxDelegate>().Hide();
			switch (type)
			{
			case MsgBoxType.BeKicked:
			case MsgBoxType.RoomDestroyed:
			case MsgBoxType.OnClosed:
				SceneName.FadeOutLevel("CoopHallTUI");
				break;
			case MsgBoxType.ContectingTimeout:
			case MsgBoxType.ContectingLost:
				SceneName.FadeOutLevel("CoopHallTUI");
				FlurryStatistics.ServerDisconnectEvent("COOP");
				break;
			case MsgBoxType.JoinRommFailed:
				break;
			}
		}
	}

	public void OnPacket(Packet packet)
	{
		uint val = 0u;
		if (packet.WatchUInt32(ref val, 4))
		{
			switch (val)
			{
			case 1048576u:
				Debug.Log("CG_HEARTBEAT revceived!!");
				break;
			case 4104u:
				Debug.Log("GC_STARTGAME revceived!!");
				OnStartGame(packet);
				break;
			}
		}
	}

	public void OnJoinRoomNotify(NetUserInfo user)
	{
		if ((bool)CellPanel)
		{
			CellPanel.RefrashClientCellShow();
		}
	}

	public void OnKickPlayer(int user_id)
	{
		if ((bool)CellPanel)
		{
			CellPanel.RefrashClientCellShow();
		}
	}

	public void OnKickPlayerNotify(int user_id)
	{
		if ((bool)CellPanel)
		{
			CellPanel.RefrashClientCellShow();
		}
	}

	public void OnLeaveRoomNotify(int user_id)
	{
		if ((bool)CellPanel)
		{
			CellPanel.RefrashClientCellShow();
		}
	}

	public void OnDestroyRoom()
	{
		SceneName.FadeOutLevel("CoopHallTUI");
	}

	public void OnDestroyRoomNotify()
	{
		if ((bool)MsgBox)
		{
			MsgBox.GetComponent<MsgBoxDelegate>().Show("Room closed, \ntry another one!", MsgBoxType.RoomDestroyed);
		}
	}

	public void OnStartGame(Packet packet)
	{
		//GCStartGamePacket gCStartGamePacket = new GCStartGamePacket();
		//if (gCStartGamePacket.ParserPacket(packet))
		//{
			GameApp.GetInstance().GetGameState().gameMode = GameMode.Coop;
			string cur_net_map = GameApp.GetInstance().GetGameState().cur_net_map;
			if (cur_net_map.Length > 0)
			{
				SceneName.FadeOutLevel(cur_net_map);
			}
			Debug.Log("Start Game Now...");
		//}
	}

	private void OnBeKicked()
	{
		if ((bool)MsgBox)
		{
			MsgBox.GetComponent<MsgBoxDelegate>().Show("You were removed from \nthe room.", MsgBoxType.BeKicked);
		}
	}

	private void OnClosed()
	{
		NetworkObj.DestroyNetCom();
		if (IndicatorPanel != null)
		{
			IndicatorPanel.GetComponent<IndicatorTUI>().Hide();
		}
		//if (MsgBox != null)
		//{
		//	MsgBox.GetComponent<MsgBoxDelegate>().Show(DialogString.netDisconnected, MsgBoxType.OnClosed);
		//}
	}

	private void OnContectingLost()
	{
		NetworkObj.DestroyNetCom();
		if (IndicatorPanel != null)
		{
			IndicatorPanel.GetComponent<IndicatorTUI>().Hide();
		}
		//if (MsgBox != null)
		//{
		//	MsgBox.GetComponent<MsgBoxDelegate>().Show(DialogString.netDisconnected, MsgBoxType.ContectingLost);
		//}
	}

	private void OnContectingTimeout()
	{
		Debug.Log("Server Connecting Timeount...");
		IndicatorPanel.GetComponent<IndicatorTUI>().Hide();
		NetworkObj.DestroyNetCom();
		//if (MsgBox != null)
		//{
		//	MsgBox.GetComponent<MsgBoxDelegate>().Show(DialogString.netUnableConnect, MsgBoxType.ContectingTimeout);
		//}
	}

	private void OnReverseHearWaiting()
	{
		if (IndicatorPanel != null)
		{
			IndicatorPanel.GetComponent<IndicatorTUI>().SetContent("WAITING FOR SERVER.");
			IndicatorPanel.GetComponent<IndicatorTUI>().Show(IndicatorTUI.IndicatorType.HeartWaiting);
		}
	}

	private void OnReverseHearRenew()
	{
		if (IndicatorPanel != null)
		{
			IndicatorPanel.GetComponent<IndicatorTUI>().Hide();
		}
	}

	private void OnReverseHearTimeout()
	{
		NetworkObj.DestroyNetCom();
		if (IndicatorPanel != null)
		{
			IndicatorPanel.GetComponent<IndicatorTUI>().Hide();
		}
		//if (MsgBox != null)
		//{
		//	MsgBox.GetComponent<MsgBoxDelegate>().Show(DialogString.netDisconnected, MsgBoxType.ContectingTimeout);
		//}
	}

	private void OnDestroy()
	{
		tnetObj.RemoveCallbacks();
		//if ((bool)net_com)
		//{
		//	net_com.UnregisterCallbacks();
		//	Debug.Log("unregister callbacks...");
		//}
	}
}
