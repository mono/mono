// CS1636: __arglist is not allowed in parameter list of iterators
// Line: 6

class C
{
    public System.Collections.IEnumerator GetEnumerator (__arglist)
    {
        yield return 1;
    }
    
}
