using UnityEngine;
using UnityEngine.UI;
using static MatchStatManager;

public class ResultRowUI : MonoBehaviour
{
    [SerializeField] private Text rankText;
    [SerializeField] private Text nicknameText;
    [SerializeField] private Text killText;
    [SerializeField] private Text deathText;

    public void SetRow(int rank, MatchPlayerStat data)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (nicknameText != null) nicknameText.text = data.Nickname;
        if (killText != null) killText.text = data.Kill.ToString();
        if (deathText != null) deathText.text = data.Death.ToString();
    }
}
