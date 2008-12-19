class A
{
	public static implicit operator byte (A mask)
	{
		return 22;
	}
}

public class Constraint
{
	const A lm = null;

	enum E1 : int { A }
	enum E2 : byte { A }

	public static Constraint operator !(Constraint m)
	{
		return null;
	}

	public static Constraint operator +(Constraint m)
	{
		return null;
	}

	public static Constraint operator ~(Constraint m)
	{
		return null;
	}

	public static Constraint operator -(Constraint m)
	{
		return null;
	}
	
	static void Foo (object o)
	{
	}
	
	public static int Main ()
	{
		
		Foo (!(Constraint)null);
		Foo (~(Constraint)null);
		Foo (+(Constraint)null);
		Foo (-(Constraint)null);
		
		const byte b1 = +0;
		const byte b2 = +b1;
		const byte b3 = (byte)0;
		const int a = -2147483648;
		const long l = -9223372036854775808;
		const long l2 = -uint.MaxValue;
		const E1 e = (E1)~E2.A;
		
		unchecked {
			if (-int.MinValue != int.MinValue)
				return 1;
		}

		int b = -lm;
		if (b != -22)
			return 2;
		
		uint ua = 2;
		if (-ua != -2)
			return 3;

		System.Console.WriteLine ("OK");
		return 0;
	}
	
}