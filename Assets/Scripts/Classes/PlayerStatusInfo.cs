class PlayerStatusInfo
{
    public int Health {get; set;}
    public int Exp {get; set;}
    public int Id {get; set;}

    public void Update(int health, int exp)
    {
        Health = health;
        Exp = exp;
    }
}