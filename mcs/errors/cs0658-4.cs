// CS0658: `)' is invalid attribute target. All attributes in this attribute section will be ignored
// Line : 9
// Compiler options: -warnaserror -warn:1

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