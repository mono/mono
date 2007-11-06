// CS0407: A method or delegate `int int.Parse(string)' return type does not match delegate `object Test.Conv(string)' return type
// Line: 17

using System;

public class Test
{
	private delegate object Conv(string str);

	public static void Main()
	{
		Conv c = new Conv(int.Parse);
	}
}
