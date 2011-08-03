// CS1657: Cannot pass `m' as a ref or out argument because it is a `using variable'
// Line: 11

using System.IO;

class E
{
    public E (int[] args)
    {
	using (MemoryStream m = new MemoryStream ()){
            Init (out m);
	}
    }
    
    void Init (out MemoryStream val)
    {
	val = null;
    }
}
