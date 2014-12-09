// Compiler options: -langversion:experimental

using System;

class RecursiveNamedPattern
{
	public static int Main ()
	{
		if (Switch_1 (null) != 4)
			return 1;

		if (Switch_1 ("x") != 5)
			return 2;

		if (Switch_1 (1) != 1)
			return 3;

//		if (Switch_1 (new C1 ()) != 3)
//			return 4;

		if (Switch_1 ((byte?) 1) != 1)
			return 5;

		if (Switch_2 (new C1 ()) != 3)
			return 10;

		if (Switch_2 (null) != 2)
			return 11;

		Console.WriteLine ("ok");
		return 0;
	}

	static int Switch_1 (object o)
	{
		switch (o) {
			case 1:
				return 1;
//			case C1 (3):
//				return 2;
//			case C1 (2):
//				return 3;
			case null:
				return 4;
			default:
				return 5;
		}
	}

	static int Switch_2 (C1 o)
	{
		switch (o) {
			case null:
				return 2;
		}

		return 3;
	}
}

public class C1
{
	public static bool operator is (C1 c1, out int i)
	{
		i = 2;
		return true;
	}
}
