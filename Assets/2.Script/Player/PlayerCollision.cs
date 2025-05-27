using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCollision : MonoBehaviourPunCallbacks
{
    
    int maxHealth = 100;
    public int curHealth;
    Material material;

    [SerializeField]
    PlayerMove playerMove;
    private void Awake()
    {
        material = GetComponentInChildren<MeshRenderer>().material;
    }
    private void Start()
    {
        curHealth = maxHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Ground")
        {
            playerMove.Land();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            StartCoroutine(OnDamage());
        }
    }

    IEnumerator OnDamage()
    {
        //피격 시 이벤트
        material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if(curHealth > 0)
        {
            material.color = Color.white;
        }
        else
        {
            playerMove.Dead();
        }
    }
}
