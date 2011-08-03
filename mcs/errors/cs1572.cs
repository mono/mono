// CS1572: XML comment on `Testing.Test.Baz(int)' has a param tag for `mismatch', but there is no parameter by that name
// Line: 10
// Compiler options: -doc:dummy.xml -warn:2 -warnaserror

namespace Testing
{
	public class Test
	{
		/// <param name='mismatch'>mismatch</param>
		public void Baz (int i) {}
	}
}
