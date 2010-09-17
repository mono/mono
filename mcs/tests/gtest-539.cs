using System;

struct S
{
	public int ID { get; set; }
}

class C
{
	public readonly int ID;

	private C (int id)
	{
		ID = id;
	}

	public static explicit operator C (S x)
	{
		throw new ApplicationException ("wrong conversion");
	}

	public static explicit operator C (S? x)
	{
		return new C (x.HasValue ? x.Value.ID : 5);
	}
}

public class Test
{
	public static int Main ()
	{
		S? s = null;
		C c = (C) s;

		if (c.ID != 5)
			return 1;

		s = new S () { ID = 10 };
		c = (C) s;

		if (c.ID != 10)
			return 2;

		return 0;
	}
}
