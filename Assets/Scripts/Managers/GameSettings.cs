using UnityEngine;

[CreateAssetMenu(menuName = "Manager/GameSettings")]

public class GameSettings: SingletonScriptableObject<MasterManager>
{
    [SerializeField] private string _gameVersion = "0.0.0";
    public string GameVersion { get { return _gameVersion; } }
    
    [SerializeField] private string _nickName = "Guest";
    public string NickName
    {
        get
        {
            int rand = Random.Range(0, 9999);
            return _nickName + rand.ToString();
        }
    }
    
    [SerializeField] private int _eachTickTime = 1;
    public int EachTickTime{ get{ return 10000*_eachTickTime;}}

    [SerializeField] private int _tickCountReset = 5;
    public int TickCountReset{ get{ return _tickCountReset;}}

    [SerializeField] private int _initialHealth = 10;
    public int InitialHealth{ get{ return _initialHealth;}}

    [SerializeField] private int _initialExp = 0;
    public int InitialExp{ get{ return _initialExp;}}

    [SerializeField] private int _leaderboardSize = 5;
    public int LeaderboardSize{ get{ return _leaderboardSize;}}

    [SerializeField] private int _heartUpgradeTicks = 10;
    public int HeartUpgradeTicks{ get{ return _heartUpgradeTicks;}}
}