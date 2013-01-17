// Compiler options: -warnaserror

using System.Diagnostics;

static class Test
{
	const bool logEnabled = false;

	[Conditional (logEnabled ? "A" : "B")]
	internal static void WriteLine (string text)
	{
	}

	public static void Main ()
	{
	}
}