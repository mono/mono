// cs1572.cs: XML comment on 'Baz' has a 'param' tag for 'mismatch', but there is no such parameter.
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
