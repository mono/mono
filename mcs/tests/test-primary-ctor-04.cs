class Derived (int arg, ref byte b, out int o) : Base (out o)
{
	public long field = arg;
	public int fieldRef = b;
}

class Base
{
	internal Base (out int o)
	{
		o = 8;
	}
}

class X
{
	public static int Main ()
	{
		int arg;
		byte b = 4;
		var d = new Derived (-5, ref b, out arg);
		if (d.field != -5)
			return 1;

		if (d.fieldRef != 4)
			return 2;

		System.Console.WriteLine ("ok");
		return 0;
	}
}