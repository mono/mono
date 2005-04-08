// CS1637: Iterators cannot have unsafe parameters or yield types
// Line: 6
// Compiler options: /unsafe

unsafe class C
{
    public System.Collections.IEnumerator GetEnumerator (int* p)
    {
        yield return 1;
    }
    
}
