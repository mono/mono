using System;

class MainClass
{
	static byte count;

	public static int Main ()
	{
		var x = Left () ?? Right();
		if (count != 1)
			return 1;

		switch (Left ()) {
		case 0:
			return 2;
		}

		if (count != 2)
			return 3;

		Console.WriteLine ("ok");
		return 0;
	}

	static int? Left()
	{
		return ++count;
	}

	static int? Right ()
	{
		return 0;
	}
}