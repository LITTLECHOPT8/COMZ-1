using System;
using System.Collections;
using UnityEngine;
using Zombie3D;
public class VSGameMissionTimer : Photon.MonoBehaviour
{
    public bool isMissionOver { get; set; }
    public float missionTotalTime { get; set; }
    public float missionCurrentTime { get; set; }
    public float missionStartTime { get; set; }
    public bool inited { get; set; }
    private PunCallback callback;
    private int lastTime;

    private void Start()
    {
        // Start the initialization process with a delay
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        // Add 1-second delay
        yield return new WaitForSeconds(3);
        Init();
    }

    public void Init()
    {
        Debug.Log("Initializing VSGameMissionTimer...");
        callback = PunCallback.CreateCallback();
        callback.server_event += TickTimer;

        isMissionOver = false;

        // Ensure the timer starts at 5:00 (300 seconds)
        missionStartTime = 0f; // Reset the start time
        missionTotalTime = 300f; // 5 minutes in seconds
        missionCurrentTime = missionTotalTime; // Start the current time at the total time
        inited = true;

        Debug.Log("Initialization complete. Start Time: " + missionStartTime + ", Current Time: " + missionCurrentTime);
    }

    public void TickTimer(ServerEventData data)
    {
        Debug.Log("TickTimer called...");
        if (data.eventCode != 9)
        {
            return;
        }

        if (data.data != null && data.data.Length > 0)
        {
            missionCurrentTime = (float)data.data[0];
            Debug.Log("Received current time: " + missionCurrentTime);
        }
        else
        {
            Debug.LogWarning("Received empty data for TickTimer.");
        }
    }

    private void Update()
    {
        if (!inited)
        {
            Debug.LogWarning("Timer not initialized.");
            return;
        }

        if (!isMissionOver)
        {
            if (PhotonNetwork.isMasterClient)
            {
                missionCurrentTime -= Time.deltaTime;
                if ((int)missionCurrentTime != lastTime)
                {
                    Debug.Log("Sending current time: " + missionCurrentTime);
                    ServerEventSystem.Send(9, new object[] { missionCurrentTime });
                    lastTime = (int)missionCurrentTime;
                }
            }

            if (missionCurrentTime <= 0f)
            {
                missionCurrentTime = 0f;
                isMissionOver = true;
                Debug.Log("Time out and game over.");
                (GameApp.GetInstance().GetGameScene() as GameVSScene).GetLastMasterKiller();
                (GameApp.GetInstance().GetGameScene() as GameVSScene).QuitGameForDisconnect(15f);
                GameApp.GetInstance().GetGameScene().GameGUI.ShowGameOverPanel(GameOverType.vsTimeOut);
            }
        }

        TimeSpan timeSpan = new TimeSpan(0, 0, (int)missionCurrentTime);
        base.gameObject.GetComponent<TUIMeshText>().text_Accessor = timeSpan.ToString();
    }
}
