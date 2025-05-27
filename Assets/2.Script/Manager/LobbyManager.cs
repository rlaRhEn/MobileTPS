using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1"; //같은 게임이라도 게임버전이 다르면 매칭이안됨

    public Text connectionInfoText;
    public Button joinButton;

    private void Start()
    {
        //서버 접속
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); //부가적인 정보 세팅

        joinButton.interactable = false;
        connectionInfoText.text = "Connecting To Server...";
    }

    public override void OnConnectedToMaster()//서버 접속
    {
        joinButton.interactable = true;
        connectionInfoText.text = "Online : connected To Server";
    }

    public override void OnDisconnected(DisconnectCause cause) //서버 접속 실패
    {
        joinButton.interactable = false;
        connectionInfoText.text = $"Offline: {cause.ToString()} ";

        PhotonNetwork.ConnectUsingSettings(); //다시 접속 시도
    }

    public void Connect() //버튼 
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

            PhotonNetwork.ConnectUsingSettings(); //다시 접속 시도
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //랜덤 방입장 실패 -자기가 방을 만들고 방장이 됨
    {
        connectionInfoText.text = "Creating Room...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8 });
    }

    public override void OnJoinedRoom() //방에 참가 성공했을 때
    {
        connectionInfoText.text = "Connected with Room";

        //방에 참가자 모두 씬 이동
        PhotonNetwork.LoadLevel("Game");
    }
}
