using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TitleCanvas _titleCanvas;

    public void FirstInitialize(TitleCanvas titleCanvas)
    {
        _titleCanvas = titleCanvas;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
