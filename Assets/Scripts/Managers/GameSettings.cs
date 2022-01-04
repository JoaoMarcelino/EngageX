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
}