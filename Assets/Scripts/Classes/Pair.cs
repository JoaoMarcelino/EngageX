public class Pair
{
    private int _new;
    private int _old;
    public Pair()
    {
        _new = 0;
        _old = 0;
    }
    public int GetNew()
    {
        return _new;
    }
    public void Add(int value)
    {
        _new += value;
    }
    public bool WasUpdated()
    {
        return _new != _old;
    }
    public void Update()
    {
        _old = _new;
    }
}