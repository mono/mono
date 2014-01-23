// Compiler options: -warnaserror -warn:4

using System;
using System.Runtime.CompilerServices;

[assembly: RuntimeCompatibility (WrapNonExceptionThrows=false)]

public class Test
{
	public static void Main ()
	{
		try {
		} catch (Exception) {
		} catch {
		}
	}
}
