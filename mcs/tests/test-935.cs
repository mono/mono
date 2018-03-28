using System;
using System.Threading.Tasks;
using System.Linq.Expressions;

public static class Program
{
	public delegate void DelegateVoid (int arg);
	public delegate int DelegateInt (string arg);

	public static int Main () 
	{ 
		Foo (Bar);

		TT (null);
		NN (0);
		NN2 (1);
		Complex (null);
		MM (1);
		MM ((byte) 1);
		DecimalRule (() => (byte) 1);
		return 0;
	}

	static void TT (Task<string> a)
	{
	}

	static void TT (Task<object> b)
	{
		throw new ApplicationException ("wrong overload");
	}

	static void NN (sbyte a)
	{
	}

	static void NN (uint? b)
	{
		throw new ApplicationException ("wrong overload");
	}

	static void NN2 (sbyte? a)
	{
	}

	static void NN2 (uint? b)
	{
		throw new ApplicationException ("wrong overload");
	}

	public static void Bar (int arg) 
	{
	}

	public static int Bar (string arg)
	{ 
		return  2;
	}

	public static void Foo (DelegateVoid input)
	{
		throw new ApplicationException ("wrong overload");
	}

	public static void Foo (DelegateInt input)
	{
	}

	static void Complex (Expression<Func<Task<short>>> arg)
	{
	}

	static void Complex (Expression<Func<Task<ulong>>> arg)
	{
		throw new ApplicationException ("wrong overload");
	}

	static void MM (double f)
	{
	}

	static void MM (double? f)
	{
		throw new ApplicationException ("wrong overload");
	}

    static void DecimalRule (Func<int> i)
    {
    }

    static void DecimalRule (Func<decimal?> i)
    {
        throw new ApplicationException ("wrong overload");
    }
}