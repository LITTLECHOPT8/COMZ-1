using System.Collections.Generic;
using UnityEngine;
using Zombie3D;

public class BonusManager : MonoBehaviour
{
	public static int bonusIndex = -1;

	public int bonusSceneIndex;

	public List<ItemType> bonusTypes;

	public GameObject bonus { get; set; }

	public int ID
	{
		get
		{
			if (bonus != null)
			{
				return bonus.GetComponent<ItemScript>().VSbonusIndex;
			}
			return -1;
		}
		set
		{
			bonus.GetComponent<ItemScript>().VSbonusIndex = value;
		}
	}

	public ItemType GetCurrentBonusType()
	{
		if (bonus != null)
		{
			return bonus.GetComponent<ItemScript>().itemType;
		}
		return ItemType.NONE;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void InitBonusObject()
	{
		int index = Random.Range(0, bonusTypes.Count);
		ItemType itemType = bonusTypes[index];
		if (bonus != null)
		{
			if (GameApp.GetInstance().GetGameState().gameMode == GameMode.Vs && bonus.GetComponent<PhotonView>().viewID != 0)
			{
				ServerEventSystem.Send(11, new object[] { bonus.GetComponent<PhotonView>().viewID }, true );
				bonus.SetActive(false);
			}
			else
			{
				Object.Destroy(bonus);
			}
			bonus = null;
		}
		bonusIndex++;
		ResourceConfigScript resourceConfig = GameApp.GetInstance().GetResourceConfig();
		if (itemType == ItemType.SuicideGun)
		{
			if (GameApp.GetInstance().GetGameState().gameMode == GameMode.Vs)
			{
				if (PhotonNetwork.isMasterClient)
				{
					bonus = PhotonNetwork.Instantiate("photon/Bonuses/" + resourceConfig.itemSuicideGun.name, base.gameObject.transform.position, Quaternion.identity, 0) as GameObject;
				}
			}
			else
			{
				bonus = Object.Instantiate(resourceConfig.itemSuicideGun, base.gameObject.transform.position, Quaternion.identity) as GameObject;
			}
		}
		else
		{
			if (GameApp.GetInstance().GetGameState().gameMode == GameMode.Vs)
			{
				if (PhotonNetwork.isMasterClient)
				{
					bonus = PhotonNetwork.Instantiate("photon/Bonuses/" + LootManagerScript.GetItemObjectFromType(itemType).name, base.gameObject.transform.position, Quaternion.identity, 0) as GameObject;
				}
			}
			else
			{
				bonus = Object.Instantiate(LootManagerScript.GetItemObjectFromType(itemType), base.gameObject.transform.position, Quaternion.identity) as GameObject;
			}
		}
		bonus.GetComponent<ItemScript>().itemType = itemType;
		bonus.GetComponent<ItemScript>().enableUpandDown = false;
		bonus.GetComponent<ItemScript>().VSbonusIndex = bonusIndex;
	}

	public void InitBonusObjectWithTypeAndId(ItemType type, int id)
	{
		if (bonus != null)
		{
			if (GameApp.GetInstance().GetGameState().gameMode == GameMode.Vs && bonus.GetComponent<PhotonView>().viewID != 0)
			{
				ServerEventSystem.Send(11, new object[] { bonus.GetComponent<PhotonView>().viewID }, true );
				bonus.SetActive(false);
			}
			else
			{
				Object.Destroy(bonus);
			}
			bonus = null;
		}
		ResourceConfigScript resourceConfig = GameApp.GetInstance().GetResourceConfig();
		if (type == ItemType.SuicideGun)
		{
			if (GameApp.GetInstance().GetGameState().gameMode == GameMode.Vs)
			{
				if (PhotonNetwork.isMasterClient)
				{
					bonus = PhotonNetwork.Instantiate("photon/Bonuses/" + resourceConfig.itemSuicideGun.name, base.gameObject.transform.position, Quaternion.identity, 0) as GameObject;
				}
			}
			else
			{
				bonus = Object.Instantiate(resourceConfig.itemSuicideGun, base.gameObject.transform.position, Quaternion.identity) as GameObject;
			}
		}
		else
		{
			if (GameApp.GetInstance().GetGameState().gameMode == GameMode.Vs)
			{
				if (PhotonNetwork.isMasterClient)
				{
					bonus = PhotonNetwork.Instantiate("photon/Bonuses/" + LootManagerScript.GetItemObjectFromType(type).name, base.gameObject.transform.position, Quaternion.identity, 0) as GameObject;
				}
			}
			else
			{
				bonus = Object.Instantiate(LootManagerScript.GetItemObjectFromType(type), base.gameObject.transform.position, Quaternion.identity) as GameObject;
			}
		}
		if (bonus == null || bonus.GetComponent<ItemScript>() == null)
		{
			Object.Destroy(bonus);
		}
		else
		{
			bonus.GetComponent<ItemScript>().itemType = type;
			bonus.GetComponent<ItemScript>().enableUpandDown = false;
			bonus.GetComponent<ItemScript>().VSbonusIndex = id;
		}
	}
}
