using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1"; //같은 게임이라도 게임버전이 다르면 매칭이안됨

    [Header("Photon")]
    public Text connectionInfoText;
    public Button joinButton;

    [Header("NickName UI")]
    public GameObject nicknamePanel;
    public InputField nicknameField;
    public Button saveNicknameButton;
    public Text nicknameInfoText;

    FirebaseFirestore db;
    bool isNicknameLoaded = false;
    
    private void Start()
    {
        joinButton.interactable = false;
        nicknamePanel.SetActive(false);

        db = FirebaseFirestore.DefaultInstance;

        //서버 접속
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings(); //부가적인 정보 세팅

        connectionInfoText.text = "서버 연결 중...";
        LoadUserProfile();
    }
    private void LoadUserProfile()
    {
        if(string.IsNullOrEmpty(UserData.Uid))
        {
            nicknameInfoText.text = "닉네임이 없습니다";
            return;
        }
        DocumentReference docRef = db.Collection("users").Document(UserData.Uid);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                nicknameInfoText.text = "유저 정보 불러오기 실패.";
                return;
            }
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                if (data.ContainsKey("nickname"))
                {
                    UserData.Nickname = data["nickname"].ToString();
                }

                if (data.ContainsKey("totalKill"))
                {
                    UserData.TotalKill = System.Convert.ToInt32(data["totalKill"]);
                }

                if (data.ContainsKey("totalDeath"))
                {
                    UserData.TotalDeath = System.Convert.ToInt32(data["totalDeath"]);
                }
            }
            if (string.IsNullOrEmpty(UserData.Nickname))
            {
                nicknamePanel.SetActive(true);
                nicknameInfoText.text = "처음 로그인입니다. 닉네임을 설정해주세요.";
            }
            else
            {
                nicknamePanel.SetActive(false);
                nicknameInfoText.text = $"닉네임: {UserData.Nickname}";
                PhotonNetwork.NickName = UserData.Nickname;
            }

            isNicknameLoaded = true;
            RefreshJoinButtonState();
        });

    }
    public void SaveNickname()
    {
        string newNickname = nicknameField.text.Trim();

        if (string.IsNullOrEmpty(newNickname))
        {
            nicknameInfoText.text = "닉네임을 입력해주세요.";
            return;
        }

        if (newNickname.Length < 2 || newNickname.Length > 12)
        {
            nicknameInfoText.text = "닉네임은 2~12자로 입력해주세요.";
            return;
        }

        saveNicknameButton.interactable = false;

        Dictionary<string, object> userData = new Dictionary<string, object>()
        {
            { "uid", UserData.Uid },
            { "email", UserData.Email },
            { "nickname", newNickname },
            { "totalKill", 0 },
            { "totalDeath", 0 },
            { "createdAt", Timestamp.GetCurrentTimestamp() }
        };

        db.Collection("users").Document(UserData.Uid).SetAsync(userData, SetOptions.MergeAll)
            .ContinueWithOnMainThread(task =>
            {
                saveNicknameButton.interactable = true;

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("닉네임 저장 실패");
                    nicknameInfoText.text = "닉네임 저장 실패";
                    return;
                }

                UserData.Nickname = newNickname;
                PhotonNetwork.NickName = newNickname;

                nicknamePanel.SetActive(false);
                nicknameInfoText.text = $"닉네임: {UserData.Nickname}";

                RefreshJoinButtonState();
            });
    }
    private void RefreshJoinButtonState()
    {
        joinButton.interactable =
            PhotonNetwork.IsConnectedAndReady &&
            isNicknameLoaded &&
            !string.IsNullOrEmpty(UserData.Nickname);
    }

    public override void OnConnectedToMaster()//서버 접속
    {
        RefreshJoinButtonState();
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
        if (string.IsNullOrEmpty(UserData.Nickname))
        {
            connectionInfoText.text = "닉네임 설정 후 입장 가능합니다.";
            return;
        }

        PhotonNetwork.NickName = UserData.Nickname;
        joinButton.interactable = false;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            connectionInfoText.text = "방 입장 중...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            connectionInfoText.text = $"다시 연결 시도 중... ";
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
