using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Players
{
	public static GameObject GetPlayer(int playerID)
	{
		foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (playerObj.GetComponent<PhotonView>().OwnerActorNr == playerID)
			{
				return playerObj;
			}
		}
		return null;
	}
}
