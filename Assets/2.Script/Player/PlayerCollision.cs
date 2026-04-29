

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerCollision : MonoBehaviourPunCallbacks
{
    int maxHealth = 100;
    public int curHealth;
    Material[] materials;

    [SerializeField] PlayerMove playerMove;
    [SerializeField] Image deathPanel;
    [SerializeField] Text respawnText;

    public string lastAttackerUid;
    public string lastAttackerNickname;

    private Coroutine damageFlashCo;

    private void Awake()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        materials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
    }

    private void Start()
    {
        curHealth = maxHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            playerMove.Land();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsMatchEnded) return;
        if (!photonView.IsMine) return;
        if (curHealth <= 0) return;
        if (!other.CompareTag("Bullet")) return;

        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet == null) return;
        if (!bullet.isInitialized) return;
        if (bullet.attackerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) return;
       

        Debug.Log($" attacker={bullet.attackerNickname}, actor={bullet.attackerActorNumber}, damage={bullet.damage}");

        lastAttackerNickname = bullet.attackerNickname;
        lastAttackerUid = bullet.attackerUid;

        curHealth -= bullet.damage;

        playerMove.photonView.RPC(nameof(PlayerMove.RpcDamageFlash), RpcTarget.All);
        StartCoroutine(OnDamage());
    }

    IEnumerator OnDamage()
    {
        if (curHealth > 0)
            yield break;

        var info = GetComponent<PlayerNetworkInfo>();

        string myUid = info != null ? info.GetUid() : UserData.Uid;
        string myNickname = info != null ? info.GetNickname() : UserData.Nickname;

        if (!string.IsNullOrEmpty(lastAttackerUid) && lastAttackerUid != myUid)
        {
            GameManager.Instance?.SyncKillToAll(lastAttackerUid, myUid, lastAttackerNickname, myNickname);
        }
        else
        {
            GameManager.Instance?.SyncKillToAll("", myUid, "", myNickname);
        }

        playerMove.Dead();

        if (deathPanel != null)
            deathPanel.gameObject.SetActive(true);

        GameManager.Instance.ReSpawnPlayer();
    }

    public void PlayDamageFlash()
    {
        if (damageFlashCo != null)
            StopCoroutine(damageFlashCo);

        damageFlashCo = StartCoroutine(CoDamageFlash());
    }

    IEnumerator CoDamageFlash()
    {
        foreach (var mat in materials)
        {
            mat.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var mat in materials)
        {
            mat.color = Color.white;
        }

        damageFlashCo = null;
    }

    public void ResetAfterRespawn()
    {
        curHealth = maxHealth;
        lastAttackerNickname = "";
        lastAttackerUid = "";

        if (deathPanel != null)
            deathPanel.gameObject.SetActive(false);

        if (respawnText != null)
            respawnText.text = "";
    }

    public void SetPanel(Image panel)
    {
        deathPanel = panel;

        if (deathPanel != null)
        {
            deathPanel.gameObject.SetActive(false);
            respawnText = deathPanel.GetComponentInChildren<Text>(true);
            if (respawnText != null) respawnText.text = "";
        }
    }

    public void SetRespawnText(Text t) => respawnText = t;

    public void UpdateRespawnUI(float remain)
    {
        if (deathPanel != null && !deathPanel.gameObject.activeSelf)
            deathPanel.gameObject.SetActive(true);

        if (respawnText != null)
            respawnText.text = $"{(int)remain}..";
    }
}