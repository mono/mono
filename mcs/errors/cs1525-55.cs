// CS1525: Unexpected symbol `)', expecting `(', `,', `.', or `]'
// Line: 8

namespace CompilerCrashWithAttributes
{
	public class Main
	{
		[MyAttribute1, MyAttribute1)]
		public Main ()
		{
		}
	}

	public class MyAttribute1 : Attribute
	{
	}
}