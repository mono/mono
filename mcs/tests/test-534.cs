// Compiler options: -t:library

// CultureTest, bugs: 79273 and 76765
[assembly: System.Reflection.AssemblyCulture("this-culture-does-not-exist")]

class X {
	public static void Main ()
	{
	}
}
