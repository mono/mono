// cs1656.cs: Cannot assign to `m' because it is a `using variable'
// Line: 10
using System.IO;

class X {

	static void Main ()
	{
		using (MemoryStream m = new MemoryStream ()){
			m = null;
		}
	}
}
	
