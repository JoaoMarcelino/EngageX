using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NicknameListing : MonoBehaviour
{
    [SerializeField] private Text _text;
    public string Nickname{get; private set;}

    public void SetNicknameText(string nickname)
    {
        Nickname = nickname;
        _text.text = nickname;
    }
}
