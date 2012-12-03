using System;

[Flags]
enum IrishBeer {
	Stout		= 0x1000,
	Ale		= 0x2000,
	Lager		= 0x3000,

	Guinness	= 1 | Stout,
	Smithwicks	= 2 | Ale
}

struct IrishPub
{
	public readonly IrishBeer Beer;

	public IrishPub (IrishBeer beer)
	{
		this.Beer = beer;
	}

	public static implicit operator long (IrishPub? pub)
	{
		return pub.HasValue ? (long) pub.Value.Beer : 0;
	}

	public static implicit operator IrishPub? (long value)
	{
		return new IrishPub ((IrishBeer) value);
	}
}

class X
{
	static int Beer (IrishPub? pub)
	{
		switch (pub) {
		case 0x1001:
			return 1;

		case 0x2002:
			return 2;

		default:
			return 3;
		}
	}

	static long PubToLong (IrishPub pub)
	{
		return pub;
	}

	static int Test (int? a)
	{
		switch (a) {
		case 0:
			return 0;

		case 3:
			return 1;

		default:
			return 2;
		}
	}

	static int TestWithNull (int? a)
	{
		switch (a) {
		case 0:
			return 0;

		case 3:
			return 1;

		case null:
			return 2;

		default:
			return 3;
		}
	}

	static long? Foo (bool flag)
	{
		if (flag)
			return 4;
		else
			return null;
	}

	static int Test (bool flag)
	{
		switch (Foo (flag)) {
		case 0:
			return 0;

		case 4:
			return 1;

		default:
			return 2;
		}
	}

	public static int Main ()
	{
		IrishPub pub = new IrishPub (IrishBeer.Guinness);
		if (PubToLong (pub) != 0x1001)
			return 1;

		if (Beer (null) != 3)
			return 2;
		if (Beer (new IrishPub (IrishBeer.Guinness)) != 1)
			return 3;

		if (Test (null) != 2)
			return 4;
		if (Test (3) != 1)
			return 5;
		if (Test (true) != 1)
			return 6;
		if (Test (false) != 2)
			return 7;

		if (TestWithNull (null) != 2)
			return 8;
		if (TestWithNull (3) != 1)
			return 9;

		return 0;
	}
}
