using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServerEventSystem
{
	public static void Send(byte eventCode, object[] data, bool all = false, bool buffer = false, bool reliable = false)
	{
		ExitGames.Client.Photon.ReceiverGroup receivers = all ? ExitGames.Client.Photon.ReceiverGroup.All : ExitGames.Client.Photon.ReceiverGroup.Others;
		ExitGames.Client.Photon.EventCaching caching = buffer ? ExitGames.Client.Photon.EventCaching.AddToRoomCache : ExitGames.Client.Photon.EventCaching.DoNotCache;
		PhotonNetwork.RaiseEvent(eventCode, data, reliable, new RaiseEventOptions { Receivers = receivers, CachingOption = caching } );
	}
}

public class ServerEventData
{
	public byte eventCode;

	public object[] data;

	public PhotonPlayer sender;
}
