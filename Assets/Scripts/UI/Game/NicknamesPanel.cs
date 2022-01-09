using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NicknamesPanel : MonoBehaviour
{
     [SerializeField] private NicknameListing _nicknameListing;

    private List<NicknameListing> _listings = new List<NicknameListing>();

    private GameCanvas _gameCanvas;
    
    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }

    public void Add(string nickname)
    {
        NicknameListing listing = (NicknameListing) Instantiate(_nicknameListing, this.transform);

        if(listing != null)
        {
            listing.SetNicknameText(nickname);
            _listings.Add(listing);
        }
    }

    public void Clear()
    {
        foreach(NicknameListing nicknameListing in _listings)
        {
            Destroy(nicknameListing.gameObject);
        }

        _listings.Clear();
    }
}
