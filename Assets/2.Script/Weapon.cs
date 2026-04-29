using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    public Transform bulletPos;
    public GameObject bulletPref;
    

    public int maxAmmo; //ÃÖ´ë ÃÑ¾Ë
    public int curAmmo; //ÇöÀç ÃÑ¾Ë

    public int damage = 20;

    Text ammoText;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            GameObject ammoObj = GameObject.Find("Ammo");
            if (ammoObj != null)
                ammoText = ammoObj.GetComponent<Text>();
        }
    }

    private void Update()
    {
        if (photonView.IsMine && ammoText != null)
        {
            ammoText.text = curAmmo + " / " + maxAmmo;
        }
    }

    public void Use(Vector3 pos)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsMatchEnded)
            return;

        if (curAmmo <= 0)
            return;

        curAmmo--;
        StartCoroutine(Shot(pos));
    }

    IEnumerator Shot(Vector3 targetPos)
    {
        Vector3 aim = (targetPos - bulletPos.position).normalized;
        Quaternion rot = Quaternion.LookRotation(aim, Vector3.up);
        Vector3 spawnPos = bulletPos.position + bulletPos.forward * 0.7f;

        object[] bulletData = new object[]
        {
        damage,
        UserData.Uid,
        UserData.Nickname,
        PhotonNetwork.LocalPlayer.ActorNumber
        };


        PhotonNetwork.Instantiate("Bullet", spawnPos, rot, 0, bulletData);
        yield return null;
    }
}
