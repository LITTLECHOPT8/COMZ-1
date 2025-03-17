using TNetSdk;
using UnityEngine;
using Zombie3D;

public class VSRoomOwnerPanel : MonoBehaviour
{
	public RoomCellData[] Client_Arr;

	private int master_id;

	private void Awake()
	{
		if (GameApp.GetInstance().GetGameState().gameMode != GameMode.Vs)
		{
			base.enabled = false;
		}
	}

	private void ClearClientsData()
	{
		RoomCellData[] client_Arr = Client_Arr;
		foreach (RoomCellData roomCellData in client_Arr)
		{
			if (roomCellData != null)
			{
				roomCellData.sfs_user = null;
				roomCellData.gameObject.transform.localPosition = new Vector3(0f, 1000f, 0f);
			}
		}
	}

	private bool SetClient(int index, PhotonPlayer player)
	{
		if (index >= 4)
		{
			Debug.LogError("index out of rang!");
			return false;
		}
		RoomCellData roomCellData = Client_Arr[index];
		if (roomCellData == null || player == null)
		{
			Debug.Log("error!:" + index);
			return false;
		}
		roomCellData.sfs_user = player;
		if (player.ID == PhotonNetwork.player.ID)
		{
			roomCellData.logo.frameName_Accessor = "Avatar_" + (int)GameApp.GetInstance().GetGameState().Avatar;
			roomCellData.level.text_Accessor = "Lv: " + GameApp.GetInstance().GetGameState().LevelNum;
			roomCellData.nickName.text_Accessor = GameApp.GetInstance().GetGameState().nick_name;
		}
		else
		{
			roomCellData.logo.frameName_Accessor = "Avatar_" + (int)player.customProperties["avatarType"];
			roomCellData.level.text_Accessor = "Lv: " + (int)player.customProperties["avatarLevel"];
			roomCellData.nickName.text_Accessor = (string)player.customProperties["nickname"];
		}
		if (TNetConnection.is_server)
		{
			if (index == 0)
			{
				roomCellData.kickButton.SetActive(false);
			}
			else
			{
				roomCellData.kickButton.SetActive(true);
			}
		}
		else
		{
			roomCellData.kickButton.SetActive(false);
		}
		return true;
	}

	private PhotonPlayer FindRoomMaster()
	{
		foreach (PhotonPlayer player in PhotonNetwork.playerList)
		{
			if (player.isMasterClient)
			{
				return player;
			}
		}
		return null;
	}

	public void RefrashClientCellShow()
	{
		ClearClientsData();
		int num = 0;
		PhotonPlayer masterClient = FindRoomMaster();
		if (masterClient == null)
		{
			return;
		}
		if (SetClient(num, masterClient))
		{
			Client_Arr[num].gameObject.transform.localPosition = new Vector3(0f, 45 - 40 * num, -1f);
			num++;
		}
		foreach (PhotonPlayer player in PhotonNetwork.playerList)
		{
			if (player.ID != masterClient.ID && SetClient(num, player))
			{
				Client_Arr[num].gameObject.transform.localPosition = new Vector3(0f, 45 - 40 * num, -1f);
				num++;
			}
		}
	}
}
