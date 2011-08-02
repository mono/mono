// CS1590: Invalid XML `include' element. Missing `file' attribute
// Line: 16
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

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

