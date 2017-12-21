using System;

public class Program
{
	public static int Main ()
	{
		int B = default (MyStruct?); 
		if (MyStruct.counter != 1)
			return 1;

		switch (default (MyStruct?)) {
			case 0:
				break;
			default:
				return 2;
		}

		if (MyStruct.counter != 2)
			return 4;

		return 0;
	}

	public struct MyStruct
	{
		public static int counter;

		public static implicit operator int (MyStruct? s)
		{
			++counter;
			return 0;
		}
	}
}