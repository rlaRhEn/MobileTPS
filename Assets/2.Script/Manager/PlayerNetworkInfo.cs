using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworkInfo : MonoBehaviourPunCallbacks
{
    private string cachedUid;
    private string cachedNickname;

    void Start()
    {
        RegisterOwner();
    }

    void RegisterOwner()
    {
        Photon.Realtime.Player owner = photonView.Owner;
        if (owner == null) return;

        string uid = "";
        string nickname = owner.NickName;

        if (owner.CustomProperties.ContainsKey("uid"))
        {
            uid = owner.CustomProperties["uid"].ToString();
        }

        if (owner.CustomProperties.ContainsKey("nickname"))
        {
            nickname = owner.CustomProperties["nickname"].ToString();
        }

        cachedUid = uid;
        cachedNickname = nickname;

        MatchStatManager.Instance?.RegisterPlayer(cachedUid, cachedNickname);
    }

    public string GetUid()
    {
        return cachedUid;
    }

    public string GetNickname()
    {
        return cachedNickname;
    }
}
