using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Bullet : MonoBehaviourPunCallbacks
{
    public int damage;

    [SerializeField]
    float speed = 10;

    float lifeTime = 2f;
    Rigidbody rigid;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        Invoke("Deactivate", lifeTime);
    }
    private void Update()
    {
        rigid.velocity = transform.forward * speed;
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Deactivate();
        }
        else if (collision.gameObject.tag == "Wall")
        {
            Deactivate();
        }
        else if (collision.gameObject.tag == "Player")
        {
            Deactivate();
        }
    }
    void Deactivate()
    {
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        CancelInvoke();
    }
}
