using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeedManager : MonoBehaviour
{
    public static KillFeedManager Instance { get; private set; }
    [SerializeField] private Transform killFeedParent;
    [SerializeField] private GameObject killLogPrefab;
    [SerializeField]private float killLogDuration = 3f;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddKillLog("TestKiller", "TestVictim");
        }
    }

    public void AddKillLog(string killer, string victim)
    {
        Debug.LogWarning($"[킬로그 생성] {killer} -> {victim}");
        if (killFeedParent == null || killLogPrefab == null)
        { 
            return;
        }

        GameObject killLog = Instantiate(killLogPrefab, killFeedParent,false);
        KillLogo killLogo = killLog.GetComponent<KillLogo>();
        if(killLogo != null)
        {
            killLogo.SetUp(killer, victim);
        }
        StartCoroutine(DestroyLog(killLog, killLogDuration));
    }
    private IEnumerator DestroyLog(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
            Destroy(obj);
    }

}
