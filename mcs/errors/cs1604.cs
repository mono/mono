// cs1604.cs: m is a readonly variable
// line: 10
using System.IO;

class X {

	static void Main ()
	{
		using (MemoryStream m = new MemoryStream ()){
			m = null;
		}
	}
}
	
