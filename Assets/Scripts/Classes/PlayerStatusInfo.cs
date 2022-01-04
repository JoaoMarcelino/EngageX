class PlayerStatusInfo
{
    public int Health {get; set;}
    public int Exp {get; set;}
    public string NickName {get; set;}

    public PlayerStatusInfo(int health, int exp, string NickName)
    {
        Health = health;
        Exp = exp;
    }
}