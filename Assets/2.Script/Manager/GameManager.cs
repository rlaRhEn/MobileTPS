using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<GameManager>();
            return instance;
        }
    }

    private static GameManager instance;

    public Transform[] spawnPositions;
    public GameObject PlayerPref;
    public PhotonObjectPoolManager photonObjectPool;

    [SerializeField] private float matchDuration = 40f;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image deathPanel;

    [SerializeField] private bool useFirestoreSave = true;

    private const string ROOM_START_TIME = "StartTime";
    private const string ROOM_MATCH_DURATION = "MatchDuration";

    private bool isMatchRunning = false;
    public bool IsMatchEnded { get; private set; }

    private FirebaseFirestore db;

    float respawnTimer = 5f;
    Coroutine respawnCo;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        SetupMatchTimer();
        SpawnPlayer();
    }

    void Update()
    {
        if (!isMatchRunning) return;

        UpdateMatchTimerUI();
    }

    void SetupMatchTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable props = new Hashtable();
            props[ROOM_START_TIME] = PhotonNetwork.Time;
            props[ROOM_MATCH_DURATION] = matchDuration;

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        isMatchRunning = true;
    }

    void UpdateMatchTimerUI()
    {
        if (PhotonNetwork.CurrentRoom == null) return;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ROOM_START_TIME)) return;
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ROOM_MATCH_DURATION)) return;

        double startTime = (double)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_START_TIME];
        float duration = (float)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_MATCH_DURATION];

        double elapsed = PhotonNetwork.Time - startTime;
        float remain = Mathf.Max(0f, duration - (float)elapsed);

        int min = Mathf.FloorToInt(remain / 60f);
        int sec = Mathf.FloorToInt(remain % 60f);

        if (timerText != null)
            timerText.text = $"{min:00}:{sec:00}";

        if (remain <= 0f)
        {
            isMatchRunning = false;

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RpcEndMatch), RpcTarget.AllViaServer);
            }
        }
    }

    void SpawnPlayer()
    {
        Hashtable props = new Hashtable();
        props["uid"] = UserData.Uid;
        props["nickname"] = UserData.Nickname;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        MatchStatManager.Instance?.RegisterPlayer(UserData.Uid, UserData.Nickname);

        var localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        var spawnPosition = spawnPositions[localPlayerIndex % spawnPositions.Length];

        GameObject character = PhotonNetwork.Instantiate(PlayerPref.name, spawnPosition.position, spawnPosition.rotation);

        if (character.GetComponent<PhotonView>().IsMine)
        {
            PlayerMove playerMove = character.GetComponent<PlayerMove>();
            var cameraArm = character.transform.Find("CameraArm");

            Command[] cmds = FindObjectsByType<Command>(FindObjectsSortMode.None);
            foreach (var cmd in cmds)
            {
                cmd.SetPlayerKey(playerMove);
            }

            FindFirstObjectByType<CameraLookDrag>()?.SetCameraArm(cameraArm);
            FindFirstObjectByType<JoyStick>()?.SetPlayerKey(playerMove);

            var pc = character.GetComponentInChildren<PlayerCollision>();
            if (pc != null)
                pc.SetPanel(deathPanel);
        }
    }

    public void ReSpawnPlayer()
    {
        if (IsMatchEnded) return;

        if (respawnCo != null)
            StopCoroutine(respawnCo);

        respawnCo = StartCoroutine(CoRespawnLocal());
    }

    IEnumerator CoRespawnLocal()
    {
        if (IsMatchEnded) yield break;

        PlayerMove localPlayer = FindLocalPlayer();
        if (localPlayer == null) yield break;

        var pc = localPlayer.GetComponentInChildren<PlayerCollision>();
        float t = respawnTimer;

        while (t > 0f)
        {
            if (pc != null)
                pc.UpdateRespawnUI(t);

            yield return null;
            t -= Time.deltaTime;
        }

        int localIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawn = spawnPositions[localIndex % spawnPositions.Length];

        localPlayer.photonView.RPC("RpcRespawn", RpcTarget.All, spawn.position, spawn.rotation);

        respawnCo = null;
    }

    PlayerMove FindLocalPlayer()
    {
        PlayerMove[] players = FindObjectsByType<PlayerMove>(FindObjectsSortMode.None);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].photonView != null && players[i].photonView.IsMine)
            {
                return players[i];
            }
        }

        return null;
    }

    [PunRPC]
    void RpcEndMatch()
    {
        if (IsMatchEnded) return;

        IsMatchEnded = true;
        Debug.Log("[Match] 제한시간 종료");

        if (respawnCo != null)
        {
            StopCoroutine(respawnCo);
            respawnCo = null;
        }

        var results = MatchStatManager.Instance?.GetSortedResults();
        if (results != null)
        {
            foreach (var r in results)
            {
                Debug.Log($"[ResultData] {r.Nickname} K={r.Kill} D={r.Death}");
            }

            ResultUIManager.Instance?.ShowResults(results);

            if (PhotonNetwork.IsMasterClient)
            {
                if (useFirestoreSave)
                {
                    SaveMatchResultToFirestore(results);
                }
            }
        }
    }

    [PunRPC]
    public void RpcOnPlayerKilled(string killerUid, string victimUid, string killerNick, string victimNick)
    {
        if (!string.IsNullOrEmpty(victimUid))
        {
            MatchStatManager.Instance?.RegisterPlayer(victimUid, victimNick);
            MatchStatManager.Instance?.AddDeath(victimUid);
        }

        if (!string.IsNullOrEmpty(killerUid) && killerUid != victimUid)
        {
            MatchStatManager.Instance?.RegisterPlayer(killerUid, killerNick);
            MatchStatManager.Instance?.AddKill(killerUid);
        }

        KillFeedManager.Instance?.AddKillLog(killerNick, victimNick);

    }

    public void SyncKillToAll(string killerUid, string victimUid, string killerNick, string victimNick)
    {
        Debug.Log($"[SyncKillToAll] killer={killerNick}, victim={victimNick}");

        if (photonView == null)
        {
            Debug.LogError("[SyncKillToAll] photonView null");
            return;
        }

        photonView.RPC(nameof(RpcOnPlayerKilled), RpcTarget.AllViaServer,
            killerUid, victimUid, killerNick, victimNick);
    }

    void SaveMatchResultToFirestore(List<MatchPlayerStat> results)
    {
        if (db == null)
        {
            Debug.LogError("[Firestore] db가 null 입니다.");
            ResultUIManager.Instance?.SetSaveStatus("저장 상태 : Firestore 초기화 실패");
            return;
        }

        if (results == null || results.Count == 0)
        {
            Debug.LogWarning("[Firestore] 저장할 결과가 없습니다.");
            ResultUIManager.Instance?.SetSaveStatus("저장 상태 : 결과 없음");
            return;
        }

        ResultUIManager.Instance?.SetSaveStatus("저장 상태 : Firestore 저장 중...");

        int pending = results.Count;
        bool hasError = false;

        foreach (var stat in results)
        {
            if (stat == null || string.IsNullOrEmpty(stat.UserId))
            {
                pending--;
                continue;
            }

            UpdateUserStat(
                stat,
                () =>
                {
                    pending--;

                    if (pending <= 0)
                    {
                        if (hasError)
                            ResultUIManager.Instance?.SetSaveStatus("저장 상태 : 일부 실패");
                        else
                            ResultUIManager.Instance?.SetSaveStatus("저장 상태 : Firestore 저장 완료");
                    }
                },
                () =>
                {
                    hasError = true;
                    pending--;

                    if (pending <= 0)
                        ResultUIManager.Instance?.SetSaveStatus("저장 상태 : Firestore 저장 실패");
                });
        }
    }

    void UpdateUserStat(MatchPlayerStat stat, System.Action onSuccess, System.Action onFail)
    {
        DocumentReference docRef = db.Collection("users").Document(stat.UserId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "uid", stat.UserId },
            { "nickname", stat.Nickname ?? "" },
            { "totalKill", FieldValue.Increment(stat.Kill) },
            { "totalDeath", FieldValue.Increment(stat.Death) }
        };

        docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"[Firestore] 저장 성공 uid={stat.UserId}, kill+={stat.Kill}, death+={stat.Death}");
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"[Firestore] 저장 실패 uid={stat.UserId}");
                if (task.Exception != null)
                    Debug.LogError(task.Exception.ToString());

                onFail?.Invoke();
            }
        });
    }

    public void ExitRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void Quit()
    {
        Application.Quit();
    }
}