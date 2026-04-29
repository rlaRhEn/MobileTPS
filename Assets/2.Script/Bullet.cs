using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Bullet : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public int damage;
    [SerializeField] float speed = 10f;
    float lifeTime = 2f;

    public string attackerUid;
    public string attackerNickname;
    public int attackerActorNumber;
    public bool isInitialized;

    Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        isInitialized = false;
        Invoke(nameof(Deactivate), lifeTime);
    }

    private void Update()
    {
        if (rigid != null)
            rigid.linearVelocity = transform.forward * speed;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;

        if (data == null)
        {
            return;
        }

        if (data.Length >= 4)
        {
            damage = (int)data[0];
            attackerUid = (string)data[1];
            attackerNickname = (string)data[2];
            attackerActorNumber = (int)data[3];
            isInitialized = true;

        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!isInitialized) return;

        if (collision.CompareTag("Ground") ||
            collision.CompareTag("Wall") ||
            collision.CompareTag("Player"))
        {
            Deactivate();
        }
    }

    void Deactivate()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        CancelInvoke();

        if (rigid != null)
            rigid.linearVelocity = Vector3.zero;
    }
}
