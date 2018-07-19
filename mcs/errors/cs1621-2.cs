// CS1621: The yield statement cannot be used inside anonymous method blocks
// Line: 12

using System;
using System.Collections;

public class Test
{
	public IEnumerator Foo ()
	{
		Call (() => {
			yield break;
		});

		yield break;
	}

	void Call (Action a)
	{
	}
}