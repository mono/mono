using System;

class B : A
{
	protected class BNested : ANested
	{
	}
}

class A : AA
{
}

class AA
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
