// Compiler options: -doc:xml-039.xml -warnaserror
using System;

/// <summary>
/// <see cref="ITest.Start" />
/// <see cref="ITest.Foo" />
/// </summary>
public interface ITest {
        /// <summary>whatever</summary>
        event EventHandler Start;
	/// <summary>hogehoge</summary>
	int Foo { get; }
}

class Test
{
	public static void Main () {}
}

