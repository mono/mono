// Compiler options: -doc:xml-032.xml -warn:4 -warnaserror
using System;

class Test
{
	/// <exception cref="ArgumentNullException"><paramref name="wrongref" /> is <see langword="null" />.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="wrongRefAgain" /> is a zero-length <see cref="string" />.</exception>
	protected Test(string name) 
	{
	}

	static void Main () {}
}

