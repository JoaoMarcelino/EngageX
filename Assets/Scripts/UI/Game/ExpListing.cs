using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpListing : MonoBehaviour
{
    [SerializeField] private Text _text;
    public string Exp{get; private set;}

    public void SetExpText(string exp)
    {
        Exp = exp;
        _text.text = exp;
    }
}