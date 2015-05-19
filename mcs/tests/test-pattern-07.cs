// Compiler options: -langversion:experimental

using System;

class PropertyPattern
{
	static int Main ()
	{
		object o = new DateTime (2014, 8, 30);

		if (!(o is DateTime { Day is 30 }))
			return 1;

		if (!(o is DateTime { Month is 8, Day is 30, Year is * }))
			return 2;

		if (o is X { Field is 30 })
			return 3;

		object o2 = new X () {
			Field = new Y () {
				Prop = 'f'
			}
		};

		bool res2 = o2 is X { Field is Y { Prop is 'f' }, Field is Y (4) };
		if (!res2)
			return 4;

		res2 = o2 is X { Field is Y { Prop is 'g' } };
		if (res2)
			return 5;

		object o3 = new X () {
			Value = 5
		};

		if (o3 is X { Value is 6 })
			return 6;

		if (!(o3 is X { Value is 5 }))
			return 7;

		object o4 = new X () {
			NullableValue = 4
		};

		bool res3 = o4 is X { NullableValue is (byte) 4 };
		if (!res3)
			return 8;

		Console.WriteLine("ok");
		return 0;
	}
}

class X
{
	public object Field { get; set; }

	public object Value { get; set; }

	public long? NullableValue { get; set; }
}

class Y
{
	public char Prop { get; set; }

	public static bool operator is (Y y, out int x)
	{
		x = 4;
		return true;
	}
}
