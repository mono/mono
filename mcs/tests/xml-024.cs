// Compiler options: -doc:xml-024.xml

namespace Testing
{
   /// <include/>
   public class Test
   {
	// warning
	/// <include file='a' />
	public static void Main ()
	{
	}

	// warning
	/// <include path='/foo/bar' />
	public void Bar (int x)
	{
	}

	// warning
	/// <include file='there-is-no-such-file' path='/foo/bar' />
	public void Baz (int x)
	{
	}
   }
}

