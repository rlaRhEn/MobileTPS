using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    public Transform bulletPos;
    public GameObject bulletPref;
    

    public int maxAmmo;
    public int curAmmo;

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
       if(curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine(Shot(pos));
        }
        else
        {

        }

    }

    IEnumerator Shot(Vector3 targetPos)
    {
        Vector3 aim = (targetPos - bulletPos.position).normalized;
        Quaternion rot = Quaternion.LookRotation(aim, Vector3.up);
        //ÃÑ¾Ë ¹ß»ç
        GameObject bullet = GameManager.Instance.photonObjectPool.SpawnFromPool("Bullet", bulletPos.position, rot);
        yield return null;
    }
    
}
