// CS1571: XML comment on `Testing.Test.Bar(int)' has a duplicate param tag for `i'
// Line: 18
// Compiler options: -doc:dummy.xml -warn:2 -warnaserror

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <param>anonymous</param>
		public void Foo (int i) {}

		/// <param name='i'>correct</param>
		/// <param name='i'>duplicate</param>
		public void Bar (int i) {}
	}
}
