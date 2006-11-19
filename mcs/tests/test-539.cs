// Compiler options: -warnaserror -warn:4

using System;

public class Test
{
	static void Main ()
	{
		try {
		} catch (Exception) {
		} catch {
		}
	}
}
