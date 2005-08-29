using System;

public class ConvFromInt {
	public int val;
	public ConvFromInt () { val = 0; }
	public ConvFromInt (int value) { val = value + 1; }
	public static implicit operator ConvFromInt (int value) { return new ConvFromInt (value); }
}

public class Foo
{
	public static ConvFromInt i = 0;
	public static object    BoolObj = (bool) false;
	public static object    ByteObj = (byte) 0;
	public static ValueType BoolVal = (bool) false;

	public static void Main ()
	{
		if (i == null) throw new Exception ("i");
		if (i.val == 0) throw new Exception ("i.val");
		if (BoolObj == null) throw new Exception ("BoolObj");
		if (ByteObj == null) throw new Exception ("ByteObj");
		if (BoolVal == null) throw new Exception ("BoolVal");
	}
}
