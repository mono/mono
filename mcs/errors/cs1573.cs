// CS1573: Parameter `j' has no matching param tag in the XML comment for `Testing.Test.Baz(int, int)'
// Line: 10
// Compiler options: -doc:dummy.xml -warn:4 -warnaserror

namespace Testing
{
	public class Test
	{
		/// <param name='i'>correct</param>
		public void Baz (int i, int j) {}
	}
}
