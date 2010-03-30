using System;

class B<T> : A<T>
{
	protected class BNested : ANested
	{
	}
}

class A<T> : AA<T>
{
}

class AA<T>
{
	protected class ANested
	{
	}
}

class M
{
	public static void Main ()
	{
	}
}
