// CS1929: Type `System.Collections.IList' does not contain a member `Frobnicate' and the best extension method overload `Extensions.Frobnicate<Test>(this Test)' requires an instance of type `Test'
// Line: 20

using System;
using System.Collections;

static class Extensions
{
	public static void Frobnicate<T> (this T foo) where T : IList
	{
	}
}

public class Test
{
	IList mFoo;

	void Frobnicate ()
	{
		mFoo.Frobnicate<Test> ();
	}
}
