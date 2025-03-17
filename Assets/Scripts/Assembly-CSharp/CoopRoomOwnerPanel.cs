using UnityEngine;
using Zombie3D;

public class CoopRoomOwnerPanel : MonoBehaviour
{
	public RoomCellData[] Client_Arr;

	public int roomUserCount;

	private void Awake()
	{
		if (GameApp.GetInstance().GetGameState().gameMode != GameMode.Coop)
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
				roomCellData.net_user = null;
				roomCellData.gameObject.transform.localPosition = new Vector3(0f, 1000f, 0f);
				roomCellData.logo.frameName_Accessor = string.Empty;
				roomCellData.level.text_Accessor = string.Empty;
				roomCellData.nickName.text_Accessor = string.Empty;
				roomCellData.kickButton.SetActive(false);
			}
		}
	}

	private bool SetClient(int index, PhotonPlayer client)
	{
		if (index >= 4)
		{
			Debug.LogError("index out of rang!");
			return false;
		}
		RoomCellData roomCellData = Client_Arr[index];
		if (roomCellData == null || client == null)
		{
			Debug.Log("error!:" + index);
			return false;
		}
		if (client.ID == PhotonNetwork.player.ID)
		{
			roomCellData.logo.frameName_Accessor = "Avatar_" + (int)GameApp.GetInstance().GetGameState().Avatar;
			roomCellData.level.text_Accessor = "Lv: " + GameApp.GetInstance().GetGameState().LevelNum;
			roomCellData.nickName.text_Accessor = GameApp.GetInstance().GetGameState().nick_name;
		}
		else
		{
			roomCellData.logo.frameName_Accessor = "Avatar_" + (int)client.customProperties["avatarType"];
			roomCellData.level.text_Accessor = "Lv: " + (int)client.customProperties["avatarLevel"];
			roomCellData.nickName.text_Accessor = (string)client.customProperties["nickname"];
		}
		roomCellData.net_user = client;
		if (PhotonNetwork.isMasterClient)
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
		roomUserCount = index + 1;
		return true;
	}

	public void RefrashClientCellShow()
	{
		ClearClientsData();
		int num = 0;
		PhotonPlayer master = PhotonNetwork.masterClient;
		if (master == null)
		{
			return;
		}
		if (SetClient(num, master))
		{
			Client_Arr[num].gameObject.transform.localPosition = new Vector3(0f, 45 - 40 * num, -1f);
			num++;
		}
		PhotonPlayer[] players = PhotonNetwork.playerList;
		foreach (PhotonPlayer player in players)
		{
			if (player != null && !player.isMasterClient && SetClient(num, player))
			{
				Client_Arr[num].gameObject.transform.localPosition = new Vector3(0f, 45 - 40 * num, -1f);
				num++;
			}
		}
	}
}
