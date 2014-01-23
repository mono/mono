using System;

class Base
{
	int value;

	public int Value {
		get { return value; }
	}

	protected Base (int value)
	{
		this.value = value;
	}
}

class A : Base
{
	public A (int value)
		: base (1)
	{
		Console.WriteLine ("Int");
	}

	public A (uint value)
		: base (2)
	{
		Console.WriteLine ("UInt");
	}
}

class B : Base
{
	public B (long value)
		: base (3)
	{
		Console.WriteLine ("Long");
	}

	public B (ulong value)
		: base (4)
	{
		Console.WriteLine ("ULong");
	}
}

class C : Base
{
	public C (short value)
		: base (5)
	{
		Console.WriteLine ("Short");
	}

	public C (ushort value)
		: base (6)
	{
		Console.WriteLine ("UShort");
	}
}

class D : Base
{
	public D (sbyte value)
		: base (7)
	{
		Console.WriteLine ("SByte");
	}

	public D (byte value)
		: base (8)
	{
		Console.WriteLine ("Byte");
	}
}

class E : Base
{
	public E (long value)
		: base (9)
	{
		Console.WriteLine ("Long");
	}

	public E (E e)
		: base (10)
	{
		Console.WriteLine ("E");
	}

	public static implicit operator E (long value) 
	{
		return (new E (value));
	}
}

class F : Base
{
	public F (int value)
		: base (11)
	{
		Console.WriteLine ("Int");
	}

	public F (F f)
		: base (12)
	{
		Console.WriteLine ("F");
	}

	public static implicit operator F (int value) 
	{
		return (new F (value));
	}
}

class X
{
	static int Test ()
	{
		{
			A a = new A (4);
			if (a.Value != 1)
				return 1;

			B b = new B (4);
			if (b.Value != 3)
				return 2;

			C c = new C (4);
			if (c.Value != 5)
				return 3;

			D d = new D (4);
			if (d.Value != 7)
				return 4;
		}

		{
			A a = new A (4u);
			if (a.Value != 2)
				return 5;

			B b = new B (4u);
			if (b.Value != 3)
				return 6;

			C c = new C (4);
			if (c.Value != 5)
				return 7;

			D d = new D (4);
			if (d.Value != 7)
				return 8;
		}

		{
			E e = new E (4);
			if (e.Value != 9)
				return 9;

			F f = new F (4);
			if (f.Value != 11)
				return 10;
		}

		return 0;
	}

	public static int Main ()
	{
		int result = Test ();
		Console.WriteLine ("RESULT: {0}", result);
		return result;
	}
}
