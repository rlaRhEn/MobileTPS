using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1"; //���� �����̶� ���ӹ����� �ٸ��� ��Ī�̾ȵ�

    public Text connectionInfoText;
    public Button joinButton;

    private void Start()
    {
        //���� ����
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); //�ΰ����� ���� ����

        joinButton.interactable = false;
        connectionInfoText.text = "Connecting To Server...";
    }

    public override void OnConnectedToMaster()//���� ����
    {
        joinButton.interactable = true;
        connectionInfoText.text = "Online : connected To Server";
    }

    public override void OnDisconnected(DisconnectCause cause) //���� ���� ����
    {
        joinButton.interactable = false;
        connectionInfoText.text = $"Offline: {cause.ToString()} ";

        PhotonNetwork.ConnectUsingSettings(); //�ٽ� ���� �õ�
    }

    public void Connect() //��ư 
    {
        joinButton.interactable = false;

        if(PhotonNetwork.IsConnected)
        {
            connectionInfoText.text = "Connecting to Room...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            connectionInfoText.text = $"Offline- Try Reconnecting ";

            PhotonNetwork.ConnectUsingSettings(); //�ٽ� ���� �õ�
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //���� ������ ���� -�ڱⰡ ���� ����� ������ ��
    {
        connectionInfoText.text = "Creating Room...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8 });
    }

    public override void OnJoinedRoom() //�濡 ���� �������� ��
    {
        connectionInfoText.text = "Connected with Room";

        //�濡 ������ ��� �� �̵�
        PhotonNetwork.LoadLevel("Game");
    }
}
