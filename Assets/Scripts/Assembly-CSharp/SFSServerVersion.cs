using System.Collections;
using System.IO;
using UnityEngine;
using Zombie3D;

public class SFSServerVersion : Photon.PunBehaviour
{
	public OnSFSServerVersion callback;

	public OnSFSServerVersionError callback_error;

    public override void OnConnectedToMaster()
    {
		PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
		if (callback != null)
		{
			callback(true);
		}
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
		callback_error();
    }

    public override void OnDisconnectedFromPhoton()
    {
        callback_error();
    }

	public void FileWrite(string FileName, string WriteString)
	{
		FileStream fileStream = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite);
		StreamWriter streamWriter = new StreamWriter(fileStream);
		streamWriter.WriteLine(WriteString);
		streamWriter.Flush();
		streamWriter.Close();
		fileStream.Close();
	}
}
