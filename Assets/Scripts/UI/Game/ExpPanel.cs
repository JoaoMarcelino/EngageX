using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPanel : MonoBehaviour
{
    [SerializeField] private ExpListing _expListing;

    private List<ExpListing> _listings = new List<ExpListing>();

    private GameCanvas _gameCanvas;
    
    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }

    public void Add(int exp)
    {
        ExpListing listing = (ExpListing) Instantiate(_expListing, this.transform);

        if(listing != null)
        {
            listing.SetExpText(exp.ToString());
            _listings.Add(listing);
        }
    }

    public void Clear()
    {
        foreach(ExpListing expListing in _listings)
        {
            Destroy(expListing.gameObject);
        }

        _listings.Clear();
    }
}
