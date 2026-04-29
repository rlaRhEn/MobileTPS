using Photon.Realtime;

public class MatchPlayerStat
{
    public string UserId;
    public string Nickname;
    public int Kill;
    public int Death;

    public MatchPlayerStat(string userId, string nickname)
    {
        UserId = userId;
        Nickname = nickname;
        Kill = 0;
        Death = 0;
    }
}
