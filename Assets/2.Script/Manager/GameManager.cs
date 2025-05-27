using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    private static GameManager instance;


    public Transform[] spawnPositions;
    public GameObject PlayerPref;
    public PhotonObjectPoolManager photonObjectPool;
    void Start()
    {
        SpawnPlayer(); //각각 플레이어 한번씩 소환

        
    }

    void SpawnPlayer()
    {
        var localPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        var spawnPosition = spawnPositions[localPlayerIndex % spawnPositions.Length];

        GameObject character = PhotonNetwork.Instantiate(PlayerPref.name, spawnPosition.position, spawnPosition.rotation);
        if (character.GetComponent<PhotonView>().IsMine)
        {
            PlayerMove playerMove = character.GetComponent<PlayerMove>();
            var cameraArm = character.transform.Find("CameraArm");

            Command[] cmds = FindObjectsOfType<Command>();
            foreach(var cmd in cmds)
            {
                cmd.SetPlayerKey(playerMove);
            }
            FindObjectOfType<CameraLookDrag>()?.SetCameraArm(cameraArm);
            FindObjectOfType<JoyStick>()?.SetPlayerKey(playerMove);

        }
    }

    public override void OnLeftRoom() //버튼 만들기
    {
        SceneManager.LoadScene("Lobby");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
