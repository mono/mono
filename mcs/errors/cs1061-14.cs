// CS1061: Type `string' does not contain a definition for `Name' and no extension method `Name' of type `string' could be found. Are you missing an assembly reference?
// Line: 18

using System;

static class X
{
	public static void Main ()
	{
	}

	static void Foo ()
	{
		var fileName = "";
		string[] all = null;

		all.Each (x => {
			var name = fileName.Name;
		});
	}

	static void Each<T> (this T[] s, Action<T> a)
	{
	}
}
