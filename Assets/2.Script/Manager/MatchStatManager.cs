using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchStatManager : MonoBehaviour
{
    public static MatchStatManager Instance;

    private Dictionary<string, MatchPlayerStat> stats = new Dictionary<string, MatchPlayerStat>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(string uid, string nickname)
    {
        if (string.IsNullOrEmpty(uid)) return;

        if (!stats.ContainsKey(uid))
        {
            stats.Add(uid, new MatchPlayerStat(uid, nickname));
            Debug.Log($"플레이어 등록 완료: {nickname} / {uid}");
        }
        else
        {
            // 닉네임 갱신 필요할 수 있어서 보정
            stats[uid].Nickname = nickname;
        }
    }

    public void AddKill(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return;

        if (stats.ContainsKey(uid))
        {
            stats[uid].Kill++;
            Debug.Log($"[MatchStat] AddKill uid={uid}, kill={stats[uid].Kill}");
        }
        else
        {
            Debug.LogWarning($"[MatchStat] AddKill 실패 - uid 없음: {uid}");
        }
    }

    public void AddDeath(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return;

        if (stats.ContainsKey(uid))
        {
            stats[uid].Death++;
            Debug.Log($"[MatchStat] AddDeath uid={uid}, death={stats[uid].Death}");
        }
        else
        {
            Debug.LogWarning($"[MatchStat] AddDeath 실패 - uid 없음: {uid}");
        }
    }

    public List<MatchPlayerStat> GetAllStats()
    {
        return new List<MatchPlayerStat>(stats.Values);
    }

    public MatchPlayerStat GetStat(string uid)
    {
        if (string.IsNullOrEmpty(uid)) return null;
        return stats.TryGetValue(uid, out var stat) ? stat : null;
    }

    public List<MatchPlayerStat> GetSortedResults()
    {
        List<MatchPlayerStat> results = new List<MatchPlayerStat>(stats.Values);

        results.Sort((a, b) =>
        {
            if (b.Kill != a.Kill)
                return b.Kill.CompareTo(a.Kill);

            return a.Death.CompareTo(b.Death);
        });

        return results;
    }

    public void ClearStats()
    {
        stats.Clear();
    }

    [Serializable]
    public class MatchPlayerResult
    {
        public string uid;
        public string nickname;
        public int kill;
        public int death;
    }
}