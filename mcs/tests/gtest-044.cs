// Operators and generic types.

using System;

class X<T>
{
	public int Count;

	public X (int count)
	{
		this.Count = count;
	}

	public static X<T> operator ++ (X<T> operand) {
		return new X<T> (operand.Count + 1);
	}
}

class Test
{
	public static void Main ()
	{
		X<long> x = new X<long> (5);
		Console.WriteLine (x.Count);
		x++;
		Console.WriteLine (x.Count);
	}
}
