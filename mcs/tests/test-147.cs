using System;

public class X
{
	public long Value = 5;
	public static long StaticValue = 6;

	public static X Foo ()
	{
		return new X ();
	}
	
	public static X Bar ()
	{
		return Foo ();
	}

	public X Baz ()
	{
		return Bar ();
	}

	public uint Property {
		get {
			return 3;
		}
	}

	public static uint StaticProperty {
		get {
			return 20;
		}
	}

	public int this [int index] {
		get {
			return 1;
		}
	}
}

public class Y : X
{
	new public long Value = 8;
	new public static long StaticValue = 9;

	public static new Y Foo ()
	{
		return new Y ();
	}

	public static new Y Bar ()
	{
		return Foo ();
	}

	public new Y Baz ()
	{
		return Bar ();
	}

	public new uint Property {
		get {
			return 4;
		}
	}

	public new static uint StaticProperty {
		get {
			return 21;
		}
	}

	public new int this [int index] {
		get {
			return 2;
		}
	}
}

public class Z : Y
{
	public int Test () {
		if (Property != 4)
			return 20;

		if (StaticProperty != 21)
			return 21;

		if (((X) this).Property != 3)
			return 22;

		if (X.StaticProperty != 20)
			return 23;

		if (this [5] != 2)
			return 24;

		if (((X) this) [6] != 1)
			return 25;

		return 0;
	}
}

public class Test
{
	public static int Main ()
	{
		Y y = new Y ();
		X a,b,c,d;

		a = Y.Bar ();
		if (!(a is Y))
			return 1;

		b = y.Baz ();
		if (!(b is Y))
			return 2;

		c = X.Bar ();
		if (c is Y)
			return 3;

		d = ((X) y).Baz ();
		if (d is Y)
			return 4;

		if (y.Value != 8)
			return 5;

		if (((X) y).Value != 5)
			return 6;

		if (Y.StaticValue != 9)
			return 7;

		if (X.StaticValue != 6)
			return 8;

		if (y.Property != 4)
			return 9;

		if (((X) y).Property != 3)
			return 10;

		if (y [5] != 2)
			return 11;

		if (((X) y) [7] != 1)
			return 10;

		if (X.StaticProperty != 20)
			return 11;

		if (Y.StaticProperty != 21)
			return 12;

		Z z = new Z ();

		return z.Test ();
	}
}
