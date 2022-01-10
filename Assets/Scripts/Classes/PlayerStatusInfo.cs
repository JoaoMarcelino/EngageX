[System.Serializable]
public class PlayerStatusInfo
{
    public int Health {get; set;}
    public int Exp {get; set;}
    public string NickName {get; set;}
    public int PlayerID {get; set;}

    public PlayerStatusInfo(int health, int exp, string nickName, int playerID)
    {
        Health = health;
        Exp = exp;
        NickName = nickName;
        PlayerID = playerID;
    }

    public void UpdateStats(int healt, int exp)
    {
        Health = Health;
        Exp = exp;
    }
}