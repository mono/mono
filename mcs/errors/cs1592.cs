// Compiler options: -doc:dummy.xml -warn:1 -warnaserror
// Badly formed XML in included comments file -- 'there-is-no-such-file'

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

