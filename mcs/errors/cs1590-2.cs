// Compiler options: -doc:xml-024.xml -warn:1 -warnaserror
// Invalid XML 'include' element; Missing 'path' attribute

namespace Testing
{
   /// comment
   public class Test
   {
	/// <include file='a' />
	public static void Main ()
	{
	}
   }
}

