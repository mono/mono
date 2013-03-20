// CS1589: Unable to include XML fragment `/foo/bar' of file `there-is-no-such-file'. Could not find file "*PATH*/there-is-no-such-file"
// Line: 12
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

namespace Testing
{
   /// blah
   public class Test
   {
	// warning
	/// <include file='there-is-no-such-file' path='/foo/bar' />
	public void Baz (int x)
	{
	}
   }
}

