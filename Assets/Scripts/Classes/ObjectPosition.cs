[System.Serializable]
public class ObjectPosition
{
    private float _x;
    private float _y;

    public float X
    {
        get { return _x;}
        set { _x = value;}
    }
    
    public float Y{
        get { return _y;}
        set { _y = value;}
    }

    public ObjectPosition(float x, float y)
    {
        _x = x;
        _y = y;
    }
}