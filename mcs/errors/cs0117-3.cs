// CS0117: `Color' does not contain a definition for `Transparent'
// Line:

using System;

struct Color
{
}

static class ExtensionMethods
{
	public static Color Transparent (this Color c)
	{
		return new Color ();
	}
}

class MainClass
{
	public static void Main ()
	{
		var c = Color.Transparent ();
	}
}
