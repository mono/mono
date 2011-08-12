// CS0210: You must provide an initializer in a fixed or using statement declaration
// Line: 14

using System;

public class C : IDisposable
{
	public void Dispose ()
	{
	}

	static void Main ()
	{
		using (C a = new C (), b) {
		}
	}
}

