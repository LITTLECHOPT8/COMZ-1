using System;
using TNetSdk;
using UnityEngine;
using Zombie3D;

public class VSGameMissionTimer : MonoBehaviour
{
	public bool isMissionOver { get; set; }

	public float missionTotalTime { get; set; }

	public float missionCurrentTime { get; set; }

	public float missionStartTime { get; set; }

	public bool inited { get; set; }

	private PunCallback callback;

	private int lastTime;

	public void Init()
	{
		callback = PunCallback.CreateCallback();
		callback.server_event += TickTimer;
		isMissionOver = false;
		missionStartTime = (float)PhotonNetwork.room.customProperties["time"];
		missionTotalTime = 600f;
		missionCurrentTime = 600f - missionStartTime;
		inited = true;
	}

	public void TickTimer(ServerEventData data)
	{
		if (data.eventCode != 9)
		{
			return;
		}
		missionCurrentTime = (float)data.data[0];
	}

	private void Update()
	{
		if (!TNetConnection.IsInitialized || !inited)
		{
			return;
		}
		if (!isMissionOver)
		{
			if (PhotonNetwork.isMasterClient)
			{
				missionCurrentTime -= Time.deltaTime;
				if ((int)missionCurrentTime != lastTime)
				{
					ServerEventSystem.Send(9, new object[] { missionCurrentTime } );
					lastTime = (int)missionCurrentTime;
				}
			}
			if (missionCurrentTime <= 0f)
			{
				missionCurrentTime = 0f;
				isMissionOver = true;
				Debug.Log("Time out and game over.");
				(GameApp.GetInstance().GetGameScene() as GameVSScene).GetLastMasterKiller();
				(GameApp.GetInstance().GetGameScene() as GameVSScene).QuitGameForDisconnect(8f);
				GameApp.GetInstance().GetGameScene().GameGUI.ShowGameOverPanel(GameOverType.vsTimeOut);
			}
		}
		TimeSpan timeSpan = new TimeSpan(0, 0, (int)missionCurrentTime);
		base.gameObject.GetComponent<TUIMeshText>().text_Accessor = timeSpan.ToString();
	}
}
