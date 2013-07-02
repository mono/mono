using System;

class Foo<T,U>
{
	public int Test (T t, U u)
	{
		return 1;
	}

	public int Test (int t, U u)
	{
		return 2;
	}

	public int Test (T t, float u)
	{
		return 3;
	}

	public int Test (int t, float u)
	{
		return 4;
	}
}

class X
{
	public static int Main ()
	{
		Foo<long,float> a = new Foo<long,float> ();
		if (a.Test (3L, 3.14F) != 3)
			return 1;
		if (a.Test (3L, 8) != 3)
			return 2;
		if (a.Test (3, 3.14F) != 4)
			return 3;
		if (a.Test (3, 8) != 4)
			return 4;

		Foo<long,double> b = new Foo<long,double> ();
		if (b.Test (3L, 3.14F) != 3)
			return 5;
		if (b.Test (3, 3.14F) != 4)
			return 6;
		if (b.Test (3L, 3.14F) != 3)
			return 7;
		if (b.Test (3L, 5) != 3)
			return 8;

		Foo<string,float> c = new Foo<string,float> ();
		if (c.Test ("Hello", 3.14F) != 3)
			return 9;

		Foo<int,string> d = new Foo<int,string> ();
		if (d.Test (3, "Hello") != 2)
			return 10;

		return 0;
	}
}
