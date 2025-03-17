using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TNetSdk;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Zombie3D
{
	public class GameMultiplayerScene : GameScene
	{
		protected int winnerId;

		protected TNetObject tnetObj;

		public Dictionary<PhotonPlayer, Player> SFS_Player_Arr = new Dictionary<PhotonPlayer, Player>();

		protected CallbackFunc game_over_call_back;

		protected float game_over_check_time;

		public bool is_game_over;

		public override bool GetGameExcute()
		{
			return is_game_excute;
		}

		public override void Init(int index)
		{
			tnetObj = TNetConnection.Connection;
			tnetObj.SetOnPlayerPropsChanged(OnPlayerPropsChanged);
			tnetObj.SetOnPlayerLeft(OnPlayerLeft);
			tnetObj.SetOnServerEvent(HandleEvent);
			//net_com = GameApp.GetInstance().GetGameState().net_com;
			//net_com.someone_birth_delegate = OnSomeoneBirth;
			//net_com.leave_room_notity_delegate = OnSomeoneLeave;
			//net_com.leave_room_delegate = OnLeaveRoom;
			game_over_call_back = OnGameOver;
			is_game_excute = true;
			//m_multi_player_arr = new List<Player>();
			//m_player_set = new List<Player>();
			base.Init(index);
			SFS_Player_Arr.Add(PhotonNetwork.player, player);
			ExitGames.Client.Photon.Hashtable table = PhotonNetwork.player.customProperties;
			table["spawnedIn"] = true;
			PhotonNetwork.player.SetCustomProperties(table);
			//m_multi_player_arr.Add(player);
			//m_player_set.Add(player);
			foreach (PhotonPlayer player in PhotonNetwork.playerList)
			{
				if (!(bool)player.customProperties["spawnedIn"])
				{
					continue;
				}
				if (player.ID != PhotonNetwork.player.ID)
				{
					BirthPlayer(player);
				}
			}
		}

		public void OnPlayerPropsChanged(object[] playerAndProps)
		{
			PhotonPlayer player = (PhotonPlayer)playerAndProps[0];
			Hashtable table = (Hashtable)playerAndProps[1];
			
			if (!(bool)table["spawnedIn"] || SFS_Player_Arr.ContainsKey(player))
			{
				return;
			}
			if (player.ID != PhotonNetwork.player.ID)
			{
				BirthPlayer(player);
			}
		}

		public void BirthPlayer(PhotonPlayer player)
		{
			if (!SFS_Player_Arr.ContainsKey(player))
			{
				Multiplayer multiplayer = new Multiplayer(player.ID);
				multiplayer.InitAvatar((AvatarType)player.customProperties["avatarType"], (uint)(int)player.customProperties["birthPoint"]);
				multiplayer.InitWeaponList((int)player.customProperties["weapon1"], (int)player.customProperties["weapon2"], (int)player.customProperties["weapon3"]);
				SFS_Player_Arr.Add(player, multiplayer);
				multiplayer.nick_name = (string)player.customProperties["nickname"];
				multiplayer.Init();
				multiplayer.ChangeWeaponWithindex((int)player.customProperties["weaponNum"]);
			}
		}

		public override void DoLogic(float deltaTime)
		{
			if (!is_game_excute)
			{
				return;
			}
			player.DoLogic(deltaTime);
			if (!is_game_over && PhotonNetwork.isMasterClient)
			{
				object[] array = new object[enemyList.Count];
				enemyList.Keys.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					Enemy enemy = enemyList[array[i]] as Enemy;
					enemy.DoLogic(deltaTime);
				}
			}
			foreach (Player player in SFS_Player_Arr.Values)
			{
				player.DoLogic(deltaTime);
			}
			game_over_check_time += deltaTime;
			if (game_over_check_time >= 5f)
			{
				game_over_check_time = 0f;
				CheckMultiGameOver();
			}
		}

		public void ResetEnemyTarget()
		{
			if (SFS_Player_Arr.Values.Count > 1)
			{
				return;
			}
			foreach (DictionaryEntry enemy in enemyList)
			{
				((Enemy)enemy.Value).SetTargetWithMultiplayer();
			}
		}

		public void HandleEvent(ServerEventData data)
		{
			switch (data.eventCode)
			{
				case 0:
					(SFS_Player_Arr[data.sender] as Multiplayer).ChangeWeaponWithindex((int)data.data[0]);
					break;

				case 1:
					if ((SFS_Player_Arr[data.sender] as Multiplayer).GetWeapon().GetWeaponType() == WeaponType.Sniper)
					{
						MultiSniper multiSniper = (SFS_Player_Arr[data.sender] as Multiplayer).GetWeapon() as MultiSniper;
						multiSniper.AddMultiTarget(new Vector3((float)data.data[0], (float)data.data[1], (float)data.data[2]));
						(SFS_Player_Arr[data.sender] as Multiplayer).OnMultiSniperFire();
					}
					break;

				case 2:
					(SFS_Player_Arr[data.sender] as Multiplayer).SetState((PlayerStateType)data.data[0]);
					break;

				case 3:
					(SFS_Player_Arr[data.sender] as Multiplayer).SetBonusStateWithType((PlayerBonusStateType)data.data[0]);
					break;

				case 4:
					ArenaTriggerBossScript.SpawnMultiEnemy((EnemyType)data.data[0], (int)data.data[1], (bool)data.data[2], (bool)data.data[3], new Vector3((float)data.data[4], (float)data.data[5], (float)data.data[6]), (bool)data.data[7], (int)data.data[8]);
					break;

				case 5:
					GameObject.Find((string)data.data[0]).name = "E_" + (int)data.data[1];
					break;

				case 6:
					Enemy enemyByID2 = GetEnemyByID((string)data.data[0]);
					if (enemyByID2 != null)
					{
						enemyByID2.OnMultiHit((double)(float)data.data[1], (WeaponType)data.data[2], (int)data.data[3]);
					}
					break;

				case 7:
					Enemy enemyByID3 = GetEnemyByID((string)data.data[0]);
					if (enemyByID3 != null && enemyByID3.GetState() != Enemy.DEAD_STATE)
					{
						enemyByID3.OnDead();
						enemyByID3.SetState(Enemy.DEAD_STATE);
					}
					break;

				case 8:
					Enemy enemyByID4 = GetEnemyByID((string)data.data[0]);
					if (enemyByID4 != null && enemyByID4.GetState() != Enemy.DEAD_STATE)
					{
						enemyByID4.OnDead();
						enemyByID4.SetState(Enemy.DEAD_STATE);
					}
					break;
				
				case 9:
					OnGameWinNotify(data.data);
					break;

				case 10:
					OnEnemyLootNotify(data.data);
					break;

				case 11:
					foreach (GameObject item in itemList)
					{
						if (item != null && item.GetComponent<ItemScript>().GameItemID == (int)data.data[0])
						{
							itemList.Remove(item);
							Object.Destroy(item);
							break;
						}
					}
					break;

				case 12:
					(SFS_Player_Arr[data.sender] as Multiplayer).OnMultiInjury((float)data.data[0], (float)data.data[1], (float)data.data[2]);
					break;

				case 13:
					if ((int)data.data[0] == player.m_multi_id)
					{
						player.OnHit((float)data.data[1] / 1000f);
					}
					break;

				case 14:
					Enemy enemyByID5 = GetEnemyByID((string)data.data[0]);
					if (enemyByID5 == null)
					{
						return;
					}
					foreach (Player item in SFS_Player_Arr.Values)
					{
						if (item.m_multi_id == (int)data.data[1])
						{
							enemyByID5.TargetPlayer = item;
							break;
						}
					}
					break;

				case 15:
					(SFS_Player_Arr[data.sender] as Multiplayer).OnRebirth();
					GameObject gameObject = Object.Instantiate(GameApp.GetInstance().GetGameResourceConfig().itemFullReviveEffect, Vector3.zero, Quaternion.identity) as GameObject;
					gameObject.transform.parent = (SFS_Player_Arr[data.sender] as Multiplayer).GetTransform();
					gameObject.transform.localPosition = Vector3.zero;
					break;

				case 16:
					if ((int)data.data[0] == 0)
					{
						GameGUI.playerInfo.SetMedpackCount();
					}
					else
					{
						GameApp.GetInstance().GetGameState().Medpack++;
					}
					break;

				case 17:
					OnUserDoRebirthNotity((SFS_Player_Arr[data.sender] as Multiplayer), data.data);
					break;

				case 18:
					foreach (Player item in SFS_Player_Arr.Values)
					{
						if (item.m_multi_id == (int)data.data[0])
						{
							item.PlayerRealDead();
							break;
						}
					}
					break;

				case 19:
					Enemy enemyByID6 = GetEnemyByID((string)data.data[0]);
					if (enemyByID6 != null)
					{
						enemyByID6.Animate((string)data.data[1], (WrapMode)(int)data.data[2]);
					}
					break;
			}
		}

		//public void OnSomeoneBirth(Player player)
		//{
		//	player.Init();
		//	m_multi_player_arr.Add(player);
		//	m_player_set.Add(player);
		//}

		//public void OnUserChangeWeaponNotify(Packet packet)
		//{
		//	GCUserChangeWeaponNotifyPacket gCUserChangeWeaponNotifyPacket = new GCUserChangeWeaponNotifyPacket();
		//	if (!gCUserChangeWeaponNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnUserChangeWeaponNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (PhotonNetwork.playerList[i] != null && PhotonNetwork.playerList[i].multiplayer != null && PhotonNetwork.playerList[i].user_id == gCUserChangeWeaponNotifyPacket.m_iUserId)
		//		{
		//			PhotonNetwork.playerList[i].multiplayer.ChangeWeaponWithindex((int)gCUserChangeWeaponNotifyPacket.m_iWeaponIndex);
		//			break;
		//		}
		//	}
		//}

		//public void OnUserSniperFireNotify(Packet packet)
		//{
		//	GCUserSniperFireNotifyPacket gCUserSniperFireNotifyPacket = new GCUserSniperFireNotifyPacket();
		//	if (!gCUserSniperFireNotifyPacket.ParserPacket(packet))
		//	{
		//		return;
		//	}
		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (PhotonNetwork.playerList[i] != null && PhotonNetwork.playerList[i].multiplayer != null && PhotonNetwork.playerList[i].user_id == gCUserSniperFireNotifyPacket.m_iUserId)
		//		{
		//			if (PhotonNetwork.playerList[i].multiplayer.GetWeapon().GetWeaponType() == WeaponType.Sniper)
		//			{
		//				MultiSniper multiSniper = PhotonNetwork.playerList[i].multiplayer.GetWeapon() as MultiSniper;
		//				multiSniper.AddMultiTarget(gCUserSniperFireNotifyPacket.m_Position);
		//				PhotonNetwork.playerList[i].multiplayer.OnMultiSniperFire();
		//			}
		//			break;
		//		}
		//	}
		//}

		//public void OnUserActionNotify(Packet packet)
		//{
		//	GCUserActionNotifyPacket gCUserActionNotifyPacket = new GCUserActionNotifyPacket();
		//	if (!gCUserActionNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnUserActionNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (PhotonNetwork.playerList[i] != null && PhotonNetwork.playerList[i].multiplayer != null && PhotonNetwork.playerList[i].user_id == gCUserActionNotifyPacket.m_iUserId)
		//		{
		//			PhotonNetwork.playerList[i].multiplayer.SetState((PlayerStateType)gCUserActionNotifyPacket.m_iAction);
		//			break;
		//		}
		//	}
		//}

		//public void OnUserBonusActionNotify(Packet packet)
		//{
		//	GCUserAuxiliaryActionNotifyPacket gCUserAuxiliaryActionNotifyPacket = new GCUserAuxiliaryActionNotifyPacket();
		//	if (!gCUserAuxiliaryActionNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnUserAuxiliaryActionNotify ParserPacekt Error!");
		//		return;
		//	}
		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (PhotonNetwork.playerList[i] != null && PhotonNetwork.playerList[i].multiplayer != null && PhotonNetwork.playerList[i].user_id == gCUserAuxiliaryActionNotifyPacket.m_iUserId)
		//		{
		//			PhotonNetwork.playerList[i].multiplayer.SetBonusStateWithType((PlayerBonusStateType)gCUserAuxiliaryActionNotifyPacket.m_iBonusAction);
		//			break;
		//		}
		//	}
		//}

		//public void OnEnemyBirthNotify(Packet packet)
		//{
		//	GCEnemyBirthNotifyPacket gCEnemyBirthNotifyPacket = new GCEnemyBirthNotifyPacket();
		//	if (!gCEnemyBirthNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnEnemyBirthNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	bool isElite = gCEnemyBirthNotifyPacket.m_isElite == 1;
		//	bool isSuperBoss = gCEnemyBirthNotifyPacket.m_isSuperBoss == 1;
		//	bool isGrave = gCEnemyBirthNotifyPacket.m_isGrave == 1;
		//	ArenaTriggerBossScript.SpawnMultiEnemy((EnemyType)gCEnemyBirthNotifyPacket.m_enemy_type, (int)gCEnemyBirthNotifyPacket.m_enemy_Id, isElite, isSuperBoss, gCEnemyBirthNotifyPacket.m_Position, isGrave, (int)gCEnemyBirthNotifyPacket.m_target_id);
		//}

		//public void OnEnemyStatusNotify(Packet packet)
		//{
		//	GCEnemyStatusNotifyPacket gCEnemyStatusNotifyPacket = new GCEnemyStatusNotifyPacket();
		//	if (!gCEnemyStatusNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnEnemyStatusNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	Enemy enemyByID = GetEnemyByID(gCEnemyStatusNotifyPacket.m_enemyID);
		//	if (enemyByID != null)
		//	{
		//		enemyByID.SetNetEnemyStatus(gCEnemyStatusNotifyPacket.m_Direction, gCEnemyStatusNotifyPacket.m_Rotation, gCEnemyStatusNotifyPacket.m_Position);
		//	}
		//}

		//public void OnEnemyGotHitNotify(Packet packet)
		//{
		//	GCEnemyGotHitNotifyPacket gCEnemyGotHitNotifyPacket = new GCEnemyGotHitNotifyPacket();
		//	if (!gCEnemyGotHitNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnEnemyGotHitNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	Enemy enemyByID = GetEnemyByID(gCEnemyGotHitNotifyPacket.m_enemyID);
		//	if (enemyByID != null)
		//	{
		//		enemyByID.OnMultiHit((double)gCEnemyGotHitNotifyPacket.m_iDamage / 1000.0, (WeaponType)gCEnemyGotHitNotifyPacket.m_weapon_type, (int)gCEnemyGotHitNotifyPacket.m_critical_attack);
		//	}
		//}

		//public void OnEnemyDeadNotify(Packet packet)
		//{
		//	GCEnemyDeadNotifyPacket gCEnemyDeadNotifyPacket = new GCEnemyDeadNotifyPacket();
		//	if (!gCEnemyDeadNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnEnemyDeadNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	Enemy enemyByID = GetEnemyByID(gCEnemyDeadNotifyPacket.enemy_id);
		//	if (enemyByID != null && enemyByID.GetState() != Enemy.DEAD_STATE)
		//	{
		//		enemyByID.OnDead();
		//		enemyByID.SetState(Enemy.DEAD_STATE);
		//	}
		//}

		//public void OnEnemyRemoveNotify(Packet packet)
		//{
		//	GCEnemyRemoveNotifyPacket gCEnemyRemoveNotifyPacket = new GCEnemyRemoveNotifyPacket();
		//	if (!gCEnemyRemoveNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnEnemyRemoveNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	Enemy enemyByID = GetEnemyByID(gCEnemyRemoveNotifyPacket.m_enemyID);
		//	if (enemyByID != null && enemyByID.GetState() != Enemy.DEAD_STATE)
		//	{
		//		enemyByID.OnDead();
		//		enemyByID.SetState(Enemy.DEAD_STATE);
		//	}
		//}

		public void OnGameWinNotify(object[] data)
		{
			//GCCoopWinnerNotifyPacket gCCoopWinnerNotifyPacket = new GCCoopWinnerNotifyPacket();
			//if (!gCCoopWinnerNotifyPacket.ParserPacket(packet))
			//{
			//	Debug.Log("OnGameWinNotify ParserPacket Error!!!");
			//	return;
			//}
			winnerId = (int)data[0];
			if (winnerId != player.m_multi_id)
			{
				GameApp.GetInstance().GetGameState().AddCash(SceneName.GetRewardFromMap(GameApp.GetInstance().GetGameState().cur_net_map));
			}
			foreach (DictionaryEntry enemy2 in enemyList)
			{
				Enemy enemy = (Enemy)enemy2.Value;
				if (!enemy.IsSuperBoss)
				{
					DamageProperty damageProperty = new DamageProperty();
					damageProperty.damage = (float)(enemy.Attributes.Hp + 10.0);
					enemy.OnHit(damageProperty, WeaponType.NoGun, false, null);
				}
			}
			foreach (Player item in SFS_Player_Arr.Values)
			{
				if (item.m_multi_id == winnerId)
				{
					player.PlayerObject.GetComponent<PlayerShell>().OnAvatarShowCameraChange(false, item);
				}
			}
			is_game_over = true;
			GameGUI.HideRebirthMsgBox();
			SaveDataReport();
			QuitGameForDisconnect(8f);
			VSReportUITemp.nextScene = "MultiReportTUI";
		}

		public void OnEnemyLootNotify(object[] data)
		{
			//GCEnemyLootNewNotifyPacket gCEnemyLootNewNotifyPacket = new GCEnemyLootNewNotifyPacket();
			//if (!gCEnemyLootNewNotifyPacket.ParserPacket(packet))
			//{
			//	Debug.Log("OnEnemyLootNotify ParserPacket Error!!!");
			//	return;
			//}
			Vector2 a = new Vector2((float)data[0], (float)data[1]);
			GameObject[] array = woodboxList;
			foreach (GameObject gameObject in array)
			{
				if (!(gameObject != null))
				{
					continue;
				}
				Vector2 b = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
				if (Vector2.Distance(a, b) < 1f)
				{
					GameObject gameObject2 = Object.Instantiate(GameApp.GetInstance().GetResourceConfig().woodExplode, gameObject.transform.position, Quaternion.identity) as GameObject;
					AudioSource component = gameObject2.GetComponent<AudioSource>();
					if (component != null)
					{
						component.mute = !GameApp.GetInstance().GetGameState().SoundOn;
					}
					Object.Destroy(gameObject);
					break;
				}
			}
			LootManagerScript.LootSpawnItem((ItemType)data[3], new Vector3((float)data[0], (float)data[1], (float)data[2]), (int)data[4]);
		}

		//public void OnPickItemNotify(Packet packet)
		//{
		//	GCPickItemNotifyPacket gCPickItemNotifyPacket = new GCPickItemNotifyPacket();
		//	if (!gCPickItemNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnPickItemNotify ParserPacket Error!!!");
		//		return;
		//	}
		//	foreach (GameObject item in itemList)
		//	{
		//		if (item != null && item.GetComponent<ItemScript>().GameItemID == (int)gCPickItemNotifyPacket.id)
		//		{
		//			itemList.Remove(item);
		//			Object.Destroy(item);
		//			break;
		//		}
		//	}
		//}

		//public void OnUserInjuryed(Packet packet)
		//{
		//	GCUserInjuryNotifyPacket gCUserInjuryNotifyPacket = new GCUserInjuryNotifyPacket();
		//	if (!gCUserInjuryNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnUserInjuryed ParserPacket Error!!!");
		//		return;
		//	}
		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (PhotonNetwork.playerList[i] != null && PhotonNetwork.playerList[i].multiplayer != null && PhotonNetwork.playerList[i].user_id == gCUserInjuryNotifyPacket.m_iUserId)
		//		{
		//			PhotonNetwork.playerList[i].multiplayer.OnMultiInjury((float)gCUserInjuryNotifyPacket.m_iInjury_val / 1000f, (float)gCUserInjuryNotifyPacket.m_total_hp_val / 1000f, (float)gCUserInjuryNotifyPacket.m_cur_hp_val / 1000f);
		//			break;
		//		}
		//	}
		//}

		//public void OnMultiplayerInjury(Packet packet)
		//{
		//	GCMultiplayerInjuryNotifyPacket gCMultiplayerInjuryNotifyPacket = new GCMultiplayerInjuryNotifyPacket();
		//	if (!gCMultiplayerInjuryNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnMultiplayerInjury ParserPacket Error!!!");
		//	}
		//	else if (gCMultiplayerInjuryNotifyPacket.m_playerId == player.m_multi_id)
		//	{
		//		player.OnHit((float)gCMultiplayerInjuryNotifyPacket.m_damage / 1000f);
		//	}
		//}

		//public void OnEnemyChangeTarget(Packet packet)
		//{
		//	GCEnemyChangeTargetNotifyPacket gCEnemyChangeTargetNotifyPacket = new GCEnemyChangeTargetNotifyPacket();
		//	if (!gCEnemyChangeTargetNotifyPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnEnemyChangeTarget ParserPacket Error!!!");
		//		return;
		//	}
		//	Enemy enemyByID = GetEnemyByID(gCEnemyChangeTargetNotifyPacket.m_enemyID);
		//	if (enemyByID == null)
		//	{
		//		return;
		//	}
		//	foreach (Player item in m_multi_player_arr)
		//	{
		//		if (item.m_multi_id == gCEnemyChangeTargetNotifyPacket.target_id)
		//		{
		//			enemyByID.TargetPlayer = item;
		//			break;
		//		}
		//	}
		//}

		//public void OnUserRebirth(Packet packet)
		//{
		//	GCUserRebirthNotifyPacket gCUserRebirthNotifyPacket = new GCUserRebirthNotifyPacket();
		//	if (!gCUserRebirthNotifyPacket.ParserPacket(packet))
		//	{
		//		return;
		//	}
		//	Debug.Log("GCUserRebirthNotifyPacket ***" + gCUserRebirthNotifyPacket.m_iUserId);
		//	for (int i = 0; i < 4; i++)
		//	{
		//		if (PhotonNetwork.playerList[i] != null && PhotonNetwork.playerList[i].multiplayer != null && PhotonNetwork.playerList[i].user_id == gCUserRebirthNotifyPacket.m_iUserId)
		//		{
		//			PhotonNetwork.playerList[i].multiplayer.OnRebirth();
		//			GameObject gameObject = Object.Instantiate(GameApp.GetInstance().GetGameResourceConfig().itemFullReviveEffect, Vector3.zero, Quaternion.identity) as GameObject;
		//			gameObject.transform.parent = PhotonNetwork.playerList[i].multiplayer.GetTransform();
		//			gameObject.transform.localPosition = Vector3.zero;
		//			break;
		//		}
		//	}
		//}

		public void OnPlayerLeft(PhotonPlayer player)
		{
			if (PhotonNetwork.isMasterClient)
			{
				OnMasterChange();
			}
			OnSomeoneLeave(player);
		}

		public void OnMasterChange()
		{
			//GCMasterChangePacket gCMasterChangePacket = new GCMasterChangePacket();
			//if (!gCMasterChangePacket.ParserPacket(packet))
			//{
			//	Debug.Log("OnMasterChange ParserPacket Error!!!");
			//	return;
			//}
//			net_com.m_netUserInfo.is_master = true;
			Enemy enemy = null;
			foreach (DictionaryEntry enemy2 in enemyList)
			{
				enemy = (Enemy)enemy2.Value;
				enemy.SetTargetWithMultiplayer();
			}
			base.EnemyID += 50;
			ArenaTrigger_Boss.CheckSpawnBoss();
			Debug.Log("OnMasterChange!!!");
		}

		//public void OnUserDoRebirth(Packet packet)
		//{
		//	GCUserDoRebirthPacket gCUserDoRebirthPacket = new GCUserDoRebirthPacket();
		//	if (gCUserDoRebirthPacket.ParserPacket(packet))
		//	{
		//		Debug.Log("OnUserDoRebirth..." + gCUserDoRebirthPacket.rebirth_user_id);
		//		if (gCUserDoRebirthPacket.m_iResult == 0)
		//		{
		//			GameGUI.playerInfo.SetMedpackCount();
		//		}
		//		else
		//		{
		//			GameApp.GetInstance().GetGameState().Medpack++;
		//		}
		//	}
		//}

		public void OnUserDoRebirthNotity(Multiplayer multiplayer, object[] data)
		{
			//Debug.Log("OnUserDoRebirthNotity...");
			//GCUserDoRebirthNotifyPacket gCUserDoRebirthNotifyPacket = new GCUserDoRebirthNotifyPacket();
			//if (!gCUserDoRebirthNotifyPacket.ParserPacket(packet))
			//{
			//	return;
			//}
			if ((int)data[0] == player.m_multi_id)
			{
				player.OnRebirth();
				GameObject gameObject = Object.Instantiate(GameApp.GetInstance().GetGameResourceConfig().itemMedpackEffect, Vector3.zero, Quaternion.identity) as GameObject;
				gameObject.transform.parent = player.GetTransform();
				gameObject.transform.localPosition = Vector3.zero;
				return;
			}
			multiplayer.OnRebirth();
			GameObject gameObject2 = Object.Instantiate(GameApp.GetInstance().GetGameResourceConfig().itemMedpackEffect, Vector3.zero, Quaternion.identity) as GameObject;
			gameObject2.transform.parent = multiplayer.GetTransform();
			gameObject2.transform.localPosition = Vector3.zero;
		}

		//public void OnUserRealDead(Packet packet)
		//{
		//	GCGameOverNotifyPacket gCGameOverNotifyPacket = new GCGameOverNotifyPacket();
		//	if (!gCGameOverNotifyPacket.ParserPacket(packet))
		//	{
		//		return;
		//	}
		//	Debug.Log("OnUserRealDead...");
		//	foreach (Player item in m_player_set)
		//	{
		//		if (item.m_multi_id == gCGameOverNotifyPacket.m_iUserId)
		//		{
		//			item.PlayerRealDead();
		//			break;
		//		}
		//	}
		//}

		public void OnSomeoneLeave(PhotonPlayer photonPlayer)
		{
			if (!is_game_excute)
			{
				return;
			}
			Debug.Log("Mutiplayer leave room : " + photonPlayer.ID);
			GameGUI.RemoveMultiplayerMiniMapMark(photonPlayer.ID);
			Player player = null;
			foreach (Player item in SFS_Player_Arr.Values)
			{
				if (item.m_multi_id == photonPlayer.ID)
				{
					player = item;
					break;
				}
			}
			if (player != null)
			{
				foreach (PhotonPlayer photonPlayer2 in SFS_Player_Arr.Keys)
				{
					if (photonPlayer2.ID == photonPlayer.ID)
					{
						SFS_Player_Arr.Remove(photonPlayer2);
					}
				}
			}
			else
			{
				GameObject gameObject = GameObject.Find("Multiplayer" + photonPlayer.ID);
				if (gameObject != null)
				{
					player = gameObject.GetComponent<PlayerShell>().m_player;
				}
			}
			if (player != null)
			{
				Enemy enemy = null;
				foreach (DictionaryEntry enemy2 in enemyList)
				{
					enemy = (Enemy)enemy2.Value;
					if (enemy.TargetPlayer != null && enemy.TargetPlayer.m_multi_id == photonPlayer.ID)
					{
						enemy.SetTargetWithMultiplayer();
						enemy.SetState(Enemy.IDLE_STATE);
					}
				}
				player.is_real_dead = true;
				Object.Destroy(player.PlayerObject);
				player = null;
			}
			OnMultiPlayerDead(null);
			CheckMultiGameOver();
		}

		public void OnLeaveRoom()
		{
			Debug.Log("Scene OnLeaveRoom...");
		}

		public void OnMultiPlayerDead(Player mPlayer)
		{
			foreach (PhotonPlayer photonPlayer in SFS_Player_Arr.Keys)
			{
				if (photonPlayer.ID == mPlayer.m_multi_id)
				{
					SFS_Player_Arr.Remove(photonPlayer);
				}
			}
		}

		public void OnGameOver(object param, object attach, bool bFinish)
		{
			SceneName.LoadLevel("VSReportTUITemp");
		}

		public void OnGameWin()
		{
			//IMPLEMENT IT!!!

			//winnerId = player.m_multi_id;
			//GameApp.GetInstance().GetGameState().AddCash(/*2 * */SceneName.GetRewardFromMap(GameApp.GetInstance().GetGameState().cur_net_map));
			//Packet packet = CGCoopWinnerPacket.MakePacket((uint)player.m_multi_id);
			//net_com.Send(packet);
			//foreach (DictionaryEntry enemy2 in enemyList)
			//{
			//	Enemy enemy = (Enemy)enemy2.Value;
			//	if (!enemy.IsSuperBoss)
			//	{
			//		DamageProperty damageProperty = new DamageProperty();
			//		damageProperty.damage = (float)(enemy.Attributes.Hp + 10.0);
			//		enemy.OnHit(damageProperty, WeaponType.NoGun, false, null);
			//	}
			//}
			//player.PlayerObject.GetComponent<PlayerShell>().OnAvatarShowCameraChange(true, player);
			//is_game_over = true;
			//SaveDataReport();
			//QuitGameForDisconnect(8f);
			//VSReportUITemp.nextScene = "MultiReportTUI";
		}

		public override void SaveDataReport()
		{
			//IMPLEMENT THIS ALSO!! 

			//Debug.Log("Scene OnGameOver...");
			//GameApp.GetInstance().GetGameState().Achievement.LoseGame();
			//GameObject gameObject = new GameObject("MultiReportData");
			//Object.DontDestroyOnLoad(gameObject);
			//MultiReportData multiReportData = gameObject.AddComponent<MultiReportData>();
			//multiReportData.play_time = Time.time - GameStartTime;
			//multiReportData.isMVP = winnerId == player.m_multi_id;
			//multiReportData.map = GameApp.GetInstance().GetGameState().cur_net_map;
			//multiReportData.avatar = player.AvatarType;
			//multiReportData.weapons = new List<string>();
			//foreach (Weapon weapon in player.weaponList)
			//{
			//	multiReportData.weapons.Add(weapon.Name);
			//}
			//int rewardFromMap = SceneName.GetRewardFromMap(GameApp.GetInstance().GetGameState().cur_net_map);
			//for (int i = 0; i < 4; i++)
			//{
			//	if (PhotonNetwork.playerList[i] == null)
			//	{
			//		continue;
			//	}
			//	if (winnerId == PhotonNetwork.playerList[i].user_id)
			//	{
			//		multiReportData.userReport.Add(i + PhotonNetwork.playerList[i].nick_name, rewardFromMap * 2);
			//	}
			//	else if (PhotonNetwork.playerList[i].multiplayer != null)
			//	{
			//		if (PhotonNetwork.playerList[i].multiplayer.PlayerObject.layer != 8)
			//		{
			//			multiReportData.userReport.Add(i + PhotonNetwork.playerList[i].nick_name, rewardFromMap / 2);
			//		}
			//		else
			//		{
			//			multiReportData.userReport.Add(i + PhotonNetwork.playerList[i].nick_name, rewardFromMap);
			//		}
			//	}
			//	else if (player.PlayerObject.layer != 8)
			//	{
			//		multiReportData.userReport.Add(i + PhotonNetwork.playerList[i].nick_name, rewardFromMap / 2);
			//	}
			//	else
			//	{
			//		multiReportData.userReport.Add(i + PhotonNetwork.playerList[i].nick_name, rewardFromMap);
			//	}
			//}
			//multiReportData.userReport.Add(" " + GameApp.GetInstance().GetGameState().GetAvatarByType(player.AvatarType).realName, rewardFromMap/* * 2*/);
			//GameGUI.gameOverPanel.GetComponent<GameOverTUI>().totalDeaths = player.m_death_count;
		}

		public void CheckMultiGameOver()
		{
			if (is_game_over)
			{
				return;
			}
			int num = 0;
			foreach (Player item in SFS_Player_Arr.Values)
			{
				if (item.is_real_dead && item.GetPlayerState() != null && item.GetPlayerState().GetStateType() == PlayerStateType.Dead)
				{
					num++;
				}
			}
			if ((num == SFS_Player_Arr.Values.Count) ? true : false)
			{
				is_game_over = true;
				QuitGameForDisconnect(5f);
				GameGUI.ShowGameOverPanel(GameOverType.coopLose);
			}
		}

		public override void TimeGameOver(float time)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/GameSceneMono")) as GameObject;
			gameObject.GetComponent<GameSceneMono>().TimerTask("GameOver", time);
		}

		public override void QuitGameForDisconnect(float time)
		{
			is_game_excute = false;
			//if (net_com != null)
			//{
			//	Packet packet = CGLeaveRoomPacket.MakePacket();
			//	GameApp.GetInstance().GetGameState().net_com.Send(packet);
			//	net_com.UnregisterCallbacks();
			//}
			NetworkObj.DestroyNetCom();
			TimeGameOver(time);
			VSReportUITemp.nextScene = "CoopHallTUI";
		}
	}
}
