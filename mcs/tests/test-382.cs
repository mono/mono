using System;
using System.Reflection;

class Dec {
	public const decimal MinValue = -79228162514264337593543950335m;
	public static void Main ()
	{
		System.Console.WriteLine ("Compiler said value is {0}", MinValue);
		FieldInfo fi = typeof (Dec).GetField ("MinValue");
		Decimal d = (Decimal) fi.GetValue (fi);
		System.Console.WriteLine ("Reflection said value is {0}", d);

		if (d != MinValue)
			throw new Exception ("decimal constant not initialized");
	}
}
