// CS0246: The type or namespace name `aGgt' could not be found. Are you missing an assembly reference?
// Line: 13

using System;

class C
{
	public static void Main ()
	{
		try {
			throw null;
		} catch (ArgumentException) {
		} catch (aGgt) {
		}
	}
}
