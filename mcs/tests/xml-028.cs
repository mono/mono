// Compiler options: -doc:xml-028.xml
using System;

/// <summary>
/// Partial comment #2
public partial class Test
{
	string Bar;

	public static void Main () {}

	/// <summary>
	/// Partial inner class!
	internal partial class Inner
	{
		public string Hoge;
	}
}

/// Partial comment #1
/// </summary>
public partial class Test
{
	public string Foo;

	/// ... is still available.
	/// </summary>
	internal partial class Inner
	{
		string Fuga;
	}
}

