//
// Compilation test
//
// This used to be a bug in the name lookups in delegate declarations
//
namespace N1
{	
	public class A
	{		
		public static int Main  ()
		{
			return 0;
		}
	}

	//
	// A used to not be resolved
	//
	public delegate void C(object sender, A a);
}
