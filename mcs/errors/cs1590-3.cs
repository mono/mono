// Compiler options: -doc:xml-024.xml -warn:1 -warnaserror
// Invalid XML 'include' element; Missing 'file' attribute

namespace Testing
{
   /// comment
   public class Test
   {
	/// comment
	public static void Main ()
	{
	}

	/// <include path='/foo/bar' />
	public void Bar (int x)
	{
	}
   }
}

