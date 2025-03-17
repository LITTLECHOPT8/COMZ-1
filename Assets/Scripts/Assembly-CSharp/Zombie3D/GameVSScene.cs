using System;
using System.Collections;
using System.Collections.Generic;
using TNetSdk;
using UnityEngine;

namespace Zombie3D
{
	public class GameVSScene : GameScene
	{
		public Dictionary<PhotonPlayer, Player> SFS_Player_Arr = new Dictionary<PhotonPlayer, Player>();

		public Dictionary<int, VSPlayerReport> SFS_Player_Report = new Dictionary<int, VSPlayerReport>();

		public float TimeToNextBonusSpawn = 15f;

		private TNetObject tnetObj;

		private float freshman_time;

		private bool is_freshman = true;

		public void PropsChanged(object[] playerAndProps)
		{
			PhotonPlayer player = (PhotonPlayer)playerAndProps[0];
			ExitGames.Client.Photon.Hashtable table = (ExitGames.Client.Photon.Hashtable)playerAndProps[1];

			if (player != PhotonNetwork.player && (bool)table["spawnedIn"] && !SFS_Player_Arr.ContainsKey(player))
			{
				OnSFSPlayerBirth(player);
			}
			else if (SFS_Player_Arr.ContainsKey(player))
			{
				OnSFSPlayerStatisticUpdate(player);
			}
		}

		public void HandleEvent(ServerEventData data)
		{
			Debug.Log("received event code " + data.eventCode + " from " + data.sender.ID);

			if (!SFS_Player_Arr.ContainsKey(data.sender))
			{
				Debug.LogWarning("PLAYER ARRAY DOES NOT CONTAIN " + data.sender.ID + " THIS IS PROBABLY NOT GOOD!!!");
				return;
			}

			switch (data.eventCode)
			{
				case 0:
					GameGUI.vsMessagePanel.AddSFSRoom((string)data.data[0]);
					break;

				case 1:
					(SFS_Player_Arr[data.sender] as Multiplayer).ChangeWeaponWithindex((int)data.data[0]);
					break;
				
				case 2:
					SFS_Player_Arr[data.sender].SetState((PlayerStateType)(int)data.data[0]);
					break;

				case 3:
					(SFS_Player_Arr[data.sender] as Multiplayer).SetBonusStateWithType((PlayerBonusStateType)(int)data.data[0]);
					break;

				case 5:
					UnityEngine.Object.Instantiate(GameApp.GetInstance().GetResourceConfig().hitBlood, ((Vector3)data.data[3]) + Vector3.up * 1f, Quaternion.identity);
					if (PhotonNetwork.player.ID != (int)data.data[0])
					{
						break;
					}
					player.OnInjuredWithUser(data.sender, (float)data.data[1], (int)data.data[2]);
					break;

				case 6:
					if (PhotonNetwork.player.ID == (int)data.data[0])
					{
						player.PlusVsKillCount();
						if (is_freshman)
						{
							is_freshman = false;
							freshman_time = Time.time - GameStartTime;
						}
						player.UpdateVSStatistic();
					}
					(SFS_Player_Arr[data.sender] as Multiplayer).OnDead();
					(SFS_Player_Arr[data.sender] as Multiplayer).SetState(PlayerStateType.Dead);
					break;

				case 7:
					(SFS_Player_Arr[data.sender] as Multiplayer).OnRebirth();
					break;

				case 8:
					int @int = (int)data.data[0];
					GameGUI.SetComboCountLabel((string)data.sender.customProperties["nickname"], @int);
					if (@int >= 8)
					{
						UnityEngine.Object.Instantiate(Resources.Load("Prefabs/narratage/8"), new Vector3(0f, 10000.1f, 0f), Quaternion.identity);
					}
					else
					{
						UnityEngine.Object.Instantiate(Resources.Load("Prefabs/narratage/" + @int), new Vector3(0f, 10000.1f, 0f), Quaternion.identity);
					}
					break;

				case 10:
					(SFS_Player_Arr[data.sender] as Multiplayer).MultiplayerSniperFire((Vector3)data.data[0]);
					break;

				case 11:
					if (PhotonNetwork.isMasterClient)
					{
						PhotonNetwork.Destroy(PhotonView.Find((int)data.data[0]));
					}
					break;

				case 12:
					GameGUI.SetComboCountLabel((string)data.sender.customProperties["nickname"], 1);
					if (!GameObject.Find("FirstBloodSound"))
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/narratage/1"), new Vector3(0f, 10000.1f, 0f), Quaternion.identity) as GameObject;
						gameObject.name = "FirstBloodSound";
					}
					break;
			}
		}

		private bool wasMaster;

		public override void Init(int index)
		{
			if (TNetConnection.IsInitialized)
			{
				tnetObj = TNetConnection.Connection;
				//tnetObj.AddEventListener(TNetEventSystem.CONNECTION_KILLED, OnConnectionLost);
				//tnetObj.AddEventListener(TNetEventSystem.DISCONNECT, OnClosed);
				//tnetObj.AddEventListener(TNetEventRoom.USER_ENTER_ROOM, OnUserEnterRoom);
				//tnetObj.AddEventListener(TNetEventRoom.USER_EXIT_ROOM, OnUserExitRoom);
				//tnetObj.AddEventListener(TNetEventRoom.OBJECT_MESSAGE, OnObjectMessage);
				//tnetObj.AddEventListener(TNetEventRoom.ROOM_VARIABLES_UPDATE, OnRoomVarsUpdate);
				//tnetObj.AddEventListener(TNetEventRoom.USER_VARIABLES_UPDATE, OnUserVarsUpdate);
				//tnetObj.AddEventListener(TNetEventRoom.LOCK_STH, OnLockSth);
				//tnetObj.AddEventListener(TNetEventRoom.ROOM_MASTER_CHANGE, OnMasterChange);
				tnetObj.SetOnPlayerLeft(OnUserExitRoom);
				tnetObj.SetOnPlayerPropsChanged(PropsChanged);
				tnetObj.SetOnRoomPropsChanged(OnRoomVarsUpdate);
				tnetObj.SetOnServerEvent(HandleEvent);
			}
			else
			{
				Debug.LogError("TNetConnection init error!");
			}
			GameApp.GetInstance().GetGameState().loot_cash = 0;
			is_game_excute = true;
			is_freshman = true;
			camera = GameObject.Find("Main Camera").GetComponent<TPSSimpleCameraScript>();
			CreateSceneBorderData();
			base.UnlockAvatar = AvatarType.None;
			base.UnlockWeapon = null;
			player = new Player();
			player.Init();
			camera.Init();
			SFS_Player_Arr[PhotonNetwork.player] = player;
			SFS_Player_Report[PhotonNetwork.player.ID] = new VSPlayerReport(GameApp.GetInstance().GetGameState().nick_name, true);
			ExitGames.Client.Photon.Hashtable table = PhotonNetwork.player.customProperties;
			table["spawnedIn"] = true;
			PhotonNetwork.player.SetCustomProperties(table);
			foreach (PhotonPlayer player in PhotonNetwork.playerList)
			{
				if (!(bool)player.customProperties["spawnedIn"])
				{
					continue;
				}
				if (player.ID != PhotonNetwork.player.ID)
				{
					OnSFSPlayerBirth(player);
				}
			}
			playingState = PlayingState.GamePlaying;
			Color[] array = new Color[8]
			{
				Color.white,
				Color.red,
				Color.blue,
				Color.yellow,
				Color.magenta,
				Color.gray,
				Color.grey,
				Color.cyan
			};
			int num = UnityEngine.Random.Range(0, array.Length);
			RenderSettings.ambientLight = array[num];
			bonusList = GameObject.FindGameObjectsWithTag("Bonus");
//			OnSFSBonusUpdate(tnetObj.CurRoom);
			enemyList = new Hashtable();
			woodboxList = new GameObject[0];
			GameStartTime = Time.time;
			wasMaster = PhotonNetwork.isMasterClient;
			GC.Collect();
		}

		public override void DoLogic(float deltaTime)
		{
			if (!is_game_excute)
			{
				return;
			}
			foreach (PhotonPlayer player in PhotonNetwork.playerList)
			{
				if (player == null || player.customProperties == null || player.customProperties["spawnedIn"] == null)
				{
					continue;
				}
				if (SFS_Player_Arr.ContainsKey(player) || !(bool)player.customProperties["spawnedIn"])
				{
					continue;
				}
				if (player.ID != PhotonNetwork.player.ID)
				{
					OnSFSPlayerBirth(player);
				}
			}
			foreach (Player value in SFS_Player_Arr.Values)
			{
				value.DoLogic(deltaTime);
			}
			PeriodicallyRespawnBonus();
		}

		public void PeriodicallyRespawnBonus()
		{
			if (PhotonNetwork.isMasterClient)
			{
				TimeToNextBonusSpawn -= Time.deltaTime;
				if (TimeToNextBonusSpawn <= 0f)
				{
					TimeToNextBonusSpawn = 30f;
					GameObject[] vSBonus = GameApp.GetInstance().GetGameScene().GetVSBonus();
					for (int i = 0; i < vSBonus.Length; i++)
					{
						BonusManager component = vSBonus[i].GetComponent<BonusManager>();
						component.InitBonusObject();
						int sceneIdx = component.bonusSceneIndex;
						if (sceneIdx == -1)
						{
							break;
						}
						int lockIdx = component.ID;
						ItemType type = component.GetCurrentBonusType();
						BonusManager component2 = GetBonusItemFromSceneIndex(sceneIdx).GetComponent<BonusManager>();
						if (type != ItemType.NONE)
						{
							if (component2.GetCurrentBonusType() != type)
							{
								component2.InitBonusObjectWithTypeAndId(type, lockIdx);
							}
						}
						else if (component2.GetCurrentBonusType() != ItemType.NONE)
						{
							if (PhotonNetwork.isMasterClient)
							{
								PhotonNetwork.Destroy(component2.bonus);
							}
							component2.bonus = null;
						}
					}
				}
			}
		}

		public void GetLastMasterKiller()
		{
			float num = -9999f;
			int num2 = -1;
			foreach (int key in SFS_Player_Report.Keys)
			{
				if (SFS_Player_Report[key].kill_cout != 0 || SFS_Player_Report[key].death_count != 0)
				{
					float num3 = (float)SFS_Player_Report[key].kill_cout - 0.6f * (float)SFS_Player_Report[key].death_count + 1.5f * (float)SFS_Player_Report[key].combo_kill;
					if (num3 > num)
					{
						num = num3;
						num2 = key;
					}
				}
			}
			if (num2 == PhotonNetwork.player.ID)
			{
				GameGUI.gameOverPanel.GetComponent<GameOverTUI>().vsChampion = true;
				player.PlayerObject.GetComponent<PlayerShell>().OnAvatarShowCameraChange(true, player);
				player.pickupItemsPacket = new Dictionary<ItemType, int>();
				player.pickupItemsPacket.Add(ItemType.Crystal, 1);
				GameGUI.ShowItemsReportPanel();
				Time.timeScale = 0f;
				return;
			}
			foreach (PhotonPlayer key2 in SFS_Player_Arr.Keys)
			{
				if (key2.ID == num2)
				{
					player.PlayerObject.GetComponent<PlayerShell>().OnAvatarShowCameraChange(false, SFS_Player_Arr[key2]);
				}
			}
		}

		public override void SaveDataReport()
		{
			GameObject gameObject = new GameObject("VSReprotObj");
			gameObject.AddComponent<VSReportData>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			gameObject.GetComponent<VSReportData>().player_reports = new List<VSPlayerReport>();
			foreach (VSPlayerReport value in SFS_Player_Report.Values)
			{
				gameObject.GetComponent<VSReportData>().player_reports.Add(value);
			}
			gameObject.GetComponent<VSReportData>().combo_kill_count = SFS_Player_Report[PhotonNetwork.player.ID].combo_kill;
			gameObject.GetComponent<VSReportData>().total_kill_count = SFS_Player_Report[PhotonNetwork.player.ID].kill_cout;
			gameObject.GetComponent<VSReportData>().freshman_time = freshman_time;
			GameGUI.gameOverPanel.GetComponent<GameOverTUI>().totalKills = SFS_Player_Report[PhotonNetwork.player.ID].kill_cout;
			GameGUI.gameOverPanel.GetComponent<GameOverTUI>().totalDeaths = SFS_Player_Report[PhotonNetwork.player.ID].death_count;
		}

		public override void QuitGameForDisconnect(float time)
		{
			is_game_excute = false;
			GameGUI.vsLabelMissionTime.GetComponent<VSGameMissionTimer>().isMissionOver = true;
			SaveDataReport();
			PhotonNetwork.LeaveRoom();
			PhotonNetwork.LeaveLobby();
			SFS_Player_Arr.Clear();
			SFS_Player_Report.Clear();
			GC.Collect();
			DestroyNetConnection();
			TimeGameOver(time);
		}

		public void OnLeaveRoom()
		{
			SceneName.LoadLevel("VSHallTUI");
		}

		public PhotonPlayer GetSFSUserFromArray(int id)
		{
			foreach (PhotonPlayer key in SFS_Player_Arr.Keys)
			{
				if (key.ID == id)
				{
					return key;
				}
			}
			return null;
		}

		private void DestroyNetConnection()
		{
			tnetObj.RemoveCallbacks();
			TNetConnection.UnregisterSFSSceneCallbacks();
			TNetConnection.Disconnect();
			tnetObj = null;
		}

		private void OnClosed(TNetEventData evt)
		{
			TNetConnection.UnregisterSFSSceneCallbacks();
			GameGUI.OnClosed();
		}

		private void OnConnectionLost(TNetEventData evt)
		{
			TNetConnection.UnregisterSFSSceneCallbacks();
			GameGUI.OnConnectingLost();
		}

		private void OnUserEnterRoom(PhotonPlayer player)
		{
			if (player.customProperties == null || player.customProperties["nickname"] == null)
			{
				return;
			}
			Debug.Log("User: " + (string)player.customProperties["nickname"] + " has just joined Room: ");
			GameGUI.vsMessagePanel.AddSFSRoom((string)player.customProperties["nickname"] + " JOINED THE GAME");
		}

		private void OnUserExitRoom(PhotonPlayer photonPlayer)
		{
			if (photonPlayer == PhotonNetwork.player)
			{
				Debug.Log("user leave room..");
				return;
			}
			Player player = null;
			if (SFS_Player_Arr.ContainsKey(photonPlayer))
			{
				player = SFS_Player_Arr[photonPlayer];
				SFS_Player_Arr.Remove(photonPlayer);
				SFS_Player_Report.Remove(photonPlayer.ID);
				GameGUI.vsMessagePanel.AddSFSRoom(((string)photonPlayer.customProperties["nickname"]) + " LEFT THE GAME");
				GameGUI.vsSeatState.RefrashSeatList(SFS_Player_Arr.Count);
			}
			if (player != null)
			{
				GameApp.GetInstance().GetGameScene().GetCamera()
					.player = base.player;
				UnityEngine.Object.Destroy(player.PlayerObject);
				player = null;
			}
			if (PhotonNetwork.isMasterClient && !wasMaster)
			{
				TimeToNextBonusSpawn = 30f;
			}
			wasMaster = PhotonNetwork.isMasterClient;
		}

		private void OnObjectMessage(TNetEventData evt)
		{
			//SFSObject sFSObject = (SFSObject)evt.data["message"];
			//TNetUser tNetUser = (TNetUser)evt.data["user"];
			//if (!SFS_Player_Arr.ContainsKey(tNetUser))
			//{
			//	return;
			//}
			//if (sFSObject.ContainsKey("msg"))
			//{
			//	GameGUI.vsMessagePanel.AddSFSRoom(sFSObject.GetUtfString("msg"));
			//}
			//else if (sFSObject.ContainsKey("comboCount"))
			//{
			//	int @int = sFSObject.GetInt("comboCount");
			//	GameGUI.SetComboCountLabel(tNetUser.Name, @int);
			//	if (@int >= 8)
			//	{
			//		UnityEngine.Object.Instantiate(Resources.Load("Prefabs/narratage/8"), new Vector3(0f, 10000.1f, 0f), Quaternion.identity);
			//	}
			//	else
			//	{
			//		UnityEngine.Object.Instantiate(Resources.Load("Prefabs/narratage/" + @int), new Vector3(0f, 10000.1f, 0f), Quaternion.identity);
			//	}
			//}
			//if (tNetUser == tnetObj.Myself)
			//{
			//	return;
			//}
			//if (sFSObject.ContainsKey("trans"))
			//{
			//	SFSObject data = sFSObject.GetSFSObject("trans") as SFSObject;
			//	if (tNetUser != null && tNetUser.Id != tnetObj.Myself.Id)
			//	{
			//		NetworkTransform ntransform = NetworkTransform.FromSFSObject(data);
			//		SFS_Player_Arr[tNetUser].networkTransform.Load(ntransform);
			//		SFS_Player_Arr[tNetUser].UpdateNetworkTrans();
			//	}
			//}
			//else if (sFSObject.ContainsKey("damage"))
			//{
			//	SFSObject sFSObject2 = sFSObject.GetSFSObject("damage") as SFSObject;
			//	float @float = sFSObject2.GetFloat("damageVal");
			//	int int2 = sFSObject2.GetInt("weaponType");
			//	SFS_Player_Arr[player.tnet_user].OnInjuredWithUser(tNetUser, @float, int2);
			//}
			//else if (sFSObject.ContainsKey("killed"))
			//{
			//	SFS_Player_Arr[player.tnet_user].PlusVsKillCount();
			//	if (is_freshman)
			//	{
			//		is_freshman = false;
			//		freshman_time = Time.time - GameStartTime;
			//	}
			//}
			//else if (sFSObject.ContainsKey("deaded"))
			//{
			//	SFS_Player_Arr[tNetUser].OnDead();
			//	SFS_Player_Arr[tNetUser].SetState(PlayerStateType.Dead);
			//}
			//else if (sFSObject.ContainsKey("rebirth"))
			//{
			//	SFS_Player_Arr[tNetUser].OnVSRebirth();
			//	NetworkTransform ntransform2 = NetworkTransform.FromSFSObject(sFSObject.GetSFSObject("rebirth"));
			//	SFS_Player_Arr[tNetUser].networkTransform.Load(ntransform2);
			//	SFS_Player_Arr[tNetUser].UpdateNetworkTrans();
			//}
			//else if (sFSObject.ContainsKey("pgmFire"))
			//{
			//	ISFSObject sFSObject3 = sFSObject.GetSFSObject("pgmFire");
			//	float float2 = sFSObject3.GetFloat("pgm_x");
			//	float float3 = sFSObject3.GetFloat("pgm_y");
			//	float float4 = sFSObject3.GetFloat("pgm_z");
			//	Multiplayer multiplayer = SFS_Player_Arr[tNetUser] as Multiplayer;
			//	multiplayer.MultiplayerSniperFire(new Vector3(float2, float3, float4));
			//}
		}

		private void OnRoomVarsUpdate(ExitGames.Client.Photon.Hashtable table)
		{
			//switch ((int)evt.data["key"])
			//{
			//case 1:
			//	OnSFSBonusUpdate(tnetObj.CurRoom);
			//	break;
			//case 2:
			//	if (tnetObj.CurRoom.GetVariable(TNetRoomVarType.firstBlood).GetBool("FirstBlood"))
			//	{
			//		string utfString = tnetObj.CurRoom.GetVariable(TNetRoomVarType.firstBlood).GetUtfString("NickName");
			//		GameGUI.SetComboCountLabel(utfString, 1);
			//		if (!GameObject.Find("FirstBloodSound"))
			//		{
			//			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/narratage/1"), new Vector3(0f, 10000.1f, 0f), Quaternion.identity) as GameObject;
			//			gameObject.name = "FirstBloodSound";
			//		}
			//	}
			//	break;
			//}
		}

		private void OnUserVarsUpdate(PhotonPlayer player)
		{
			//TNetUserVarType tNetUserVarType = (TNetUserVarType)(int)evt.data["key"];
			//TNetUser tNetUser = (TNetUser)evt.data["user"];
			//if (tNetUser == tnetObj.Myself && tNetUserVarType != TNetUserVarType.userStatistics)
			//{
			//	return;
			//}
			//if (!SFS_Player_Arr.ContainsKey(tNetUser) && tNetUserVarType == TNetUserVarType.avatarData && tNetUser.Id != tnetObj.Myself.Id)
			//{
			//	OnSFSPlayerBirth(tNetUser);
			//	return;
			//}
			//if (tNetUserVarType == TNetUserVarType.CurWeapon && SFS_Player_Arr.ContainsKey(tNetUser))
			//{
			//	((Multiplayer)SFS_Player_Arr[tNetUser]).ChangeWeaponWithindex(tNetUser.GetVariable(TNetUserVarType.CurWeapon).GetInt("data"));
			//}
			//if (tNetUserVarType == TNetUserVarType.PlayerState && SFS_Player_Arr.ContainsKey(tNetUser))
			//{
			//	SFS_Player_Arr[tNetUser].SetState((PlayerStateType)tNetUser.GetVariable(TNetUserVarType.PlayerState).GetInt("data"));
			//}
			//if (tNetUserVarType == TNetUserVarType.PlayerBonusState && SFS_Player_Arr.ContainsKey(tNetUser))
			//{
			//	((Multiplayer)SFS_Player_Arr[tNetUser]).SetBonusStateWithType((PlayerBonusStateType)tNetUser.GetVariable(TNetUserVarType.PlayerBonusState).GetInt("data"));
			//}
			//if (tNetUserVarType == TNetUserVarType.userStatistics && SFS_Player_Arr.ContainsKey(tNetUser))
			//{
			//	OnSFSPlayerStatisticUpdate(tNetUser);
			//}
		}

		private void OnSFSPlayerBirth(PhotonPlayer user)
		{
			if (!SFS_Player_Arr.ContainsKey(user))
			{
				ServerEventSystem.Send(1, new object[] { (int)PhotonNetwork.player.customProperties["weaponNum"] } );
				ServerEventSystem.Send(2, new object[] { (int)player.GetPlayerState().GetStateType() } );
				ServerEventSystem.Send(3, new object[] { (int)player.PlayerBonusState.StateType } );

				Debug.Log("OnSFSPlayerBirth name:" + user.ToString());
				Multiplayer multiplayer = new Multiplayer(user.ID);
				multiplayer.nick_name = (string)user.customProperties["nickname"];
				multiplayer.InitAvatar((AvatarType)user.customProperties["avatarType"], 0u);
				multiplayer.InitWeaponList((int)user.customProperties["weapon1"], (float)user.customProperties["weaponPara1"], (int)user.customProperties["weapon2"], (float)user.customProperties["weaponPara2"], (int)user.customProperties["weapon3"], (float)user.customProperties["weaponPara3"]);
				multiplayer.birth_point_index = 0;
				multiplayer.Init();
				multiplayer.tnet_user = user;
				multiplayer.ChangeWeaponWithindex((int)user.customProperties["weaponNum"]);
				multiplayer.SetState((PlayerStateType)(int)user.customProperties["playerState"]);
				multiplayer.SetBonusStateWithType((PlayerBonusStateType)(int)user.customProperties["playerBonusState"]);
				SFS_Player_Arr[user] = multiplayer;
				if (!SFS_Player_Report.ContainsKey(user.ID))
				{
					SFS_Player_Report[user.ID] = new VSPlayerReport(multiplayer.nick_name, false);
				}
				GameUIScriptNew.GetGameUIScript().vsSeatState.RefrashSeatList(SFS_Player_Arr.Count);
				OnUserEnterRoom(user);
			}
		}

		private void OnSFSBonusUpdate(TNetRoom room)
		{
			if (!room.ContainsVariable(TNetRoomVarType.BonusInfo))
			{
				return;
			}
			ISFSArray sFSArray = room.GetVariable(TNetRoomVarType.BonusInfo).GetSFSArray("data");
			for (int i = 0; i < sFSArray.Size(); i++)
			{
				ISFSObject sFSObject = sFSArray.GetSFSObject(i);
				int @int = sFSObject.GetInt("sceneIdx");
				if (@int == -1)
				{
					break;
				}
				int int2 = sFSObject.GetInt("lockIdx");
				ItemType int3 = (ItemType)sFSObject.GetInt("type");
				BonusManager component = GetBonusItemFromSceneIndex(@int).GetComponent<BonusManager>();
				if (int3 != ItemType.NONE)
				{
					if (component.GetCurrentBonusType() != int3)
					{
						component.InitBonusObjectWithTypeAndId(int3, int2);
					}
				}
				else if (component.GetCurrentBonusType() != ItemType.NONE)
				{
					UnityEngine.Object.Destroy(component.bonus);
					component.bonus = null;
				}
			}
		}

		private GameObject GetBonusItemFromLockIndex(string idx)
		{
			if (bonusList == null || bonusList.Length == 0)
			{
				bonusList = GameObject.FindGameObjectsWithTag("Bonus");
			}
			for (int i = 0; i < bonusList.Length; i++)
			{
				if (bonusList[i].GetComponent<BonusManager>().ID.ToString() == idx)
				{
					return bonusList[i];
				}
			}
			return null;
		}

		private GameObject GetBonusItemFromSceneIndex(int idx)
		{
			if (bonusList == null || bonusList.Length == 0)
			{
				bonusList = GameObject.FindGameObjectsWithTag("Bonus");
			}
			for (int i = 0; i < bonusList.Length; i++)
			{
				if (bonusList[i].GetComponent<BonusManager>().bonusSceneIndex == idx)
				{
					return bonusList[i];
				}
			}
			return null;
		}

		private void OnSFSPlayerStatisticUpdate(PhotonPlayer player)
		{
			if (!SFS_Player_Report.ContainsKey(player.ID))
			{
				Debug.LogError("OnSFSPlayerStatisticUpdate not contain key:" + player.ID);
				return;
			}
			SFS_Player_Report[player.ID].kill_cout = (int)player.customProperties["killCount"];
			SFS_Player_Report[player.ID].death_count = (int)player.customProperties["deathCount"];
			SFS_Player_Report[player.ID].loot_cash = (int)player.customProperties["cashLoot"];
			SFS_Player_Report[player.ID].combo_kill = (int)player.customProperties["vsCombo"];
			CheckFirstBlood();
			RefrashMasterKiller();
		}

		private void CheckFirstBlood()
		{
			//if ((bool)PhotonNetwork.room.customProperties["firstBlood"])
			//{
			//	return;
			//}
			//string val = string.Empty;
			//int num = 0;
			//foreach (VSPlayerReport value in SFS_Player_Report.Values)
			//{
			//	num += value.kill_cout;
			//	if (value.kill_cout == 1)
			//	{
			//		val = value.nick_name;
			//	}
			//}
			//if (num == 1)
			//{
			//	ExitGames.Client.Photon.Hashtable table = PhotonNetwork.room.customProperties;
			//	table["firstBlood"] = true;
			//	table["firstBloodUser"] = GameApp.GetInstance().GetGameState().nick_name;
			//	PhotonNetwork.room.SetCustomProperties(table);
			//}
		}

		private void RefrashMasterKiller()
		{
			List<int> list = new List<int>();
			int num = -1;
			foreach (int key in SFS_Player_Report.Keys)
			{
				if (SFS_Player_Report[key].kill_cout > num)
				{
					num = SFS_Player_Report[key].kill_cout;
				}
			}
			foreach (int key2 in SFS_Player_Report.Keys)
			{
				if (SFS_Player_Report[key2].kill_cout >= num)
				{
					list.Add(key2);
				}
			}
			foreach (PhotonPlayer key3 in SFS_Player_Arr.Keys)
			{
				if (key3.ID != PhotonNetwork.player.ID)
				{
					if (list.Contains(key3.ID))
					{
						(SFS_Player_Arr[key3] as Multiplayer).NickNameLabel.GetComponent<TUIMeshTextFx>().color_Accessor = Color.red;
					}
					else
					{
						(SFS_Player_Arr[key3] as Multiplayer).NickNameLabel.GetComponent<TUIMeshTextFx>().color_Accessor = ColorName.GetPlayerMarkColor((int)SFS_Player_Arr[key3].birth_point_index);
					}
				}
			}
		}

		private void OnLockSth(TNetEventData evt)
		{
			if ((int)evt.data["result"] != 0)
			{
				return;
			}
			string text = (string)evt.data["key"];
			if (GetBonusItemFromLockIndex(text) != null)
			{
				BonusManager component = GetBonusItemFromLockIndex(text).GetComponent<BonusManager>();
				if (component != null && component.bonus != null && player.OnPickUp(component.GetCurrentBonusType()))
				{
					player.UpdateAndBroadcastBonusInfo(false, text);
				}
			}
		}

		private void OnMasterChange(TNetEventData evt)
		{
			TNetUser tNetUser = (TNetUser)evt.data["user"];
			if (tNetUser != null)
			{
				Debug.Log("OnMasterChange...");
				if (tNetUser.Id == tnetObj.Myself.Id)
				{
					TimeToNextBonusSpawn = 30f;
				}
			}
		}

		public override void TimeGameOver(float time)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/GameSceneMono")) as GameObject;
			gameObject.GetComponent<GameSceneMono>().TimerTask("VSGameOver", time);
		}

		public void OnGameOver(object param, object attach, bool bFinish)
		{
			VSReportUITemp.nextScene = "VSReportTUI";
			GameApp.GetInstance().ClearScene();
			SceneName.LoadLevel("VSReportTUITemp");
		}
	}
}
