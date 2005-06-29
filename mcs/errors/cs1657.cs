// cs1657.cs: Cannot pass `i' as a ref or out argument because it is a `foreach iteration variable'
// Line: 9

class E
{
    public E (int[] args)
    {
        foreach (int i in args)
            Init (ref i);
    }
    
    void Init (ref int val) {}
       
}
