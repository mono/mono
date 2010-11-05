// Compiler options: -t:library

using System;

public class A
{
	public class AN
	{
		public B.N TestReturn ()
		{
			return new B.N ();
		}
	}
}

public class B
{
	public class N : C.N
	{
	}
}

public class C
{
	public class N
	{
		public void Test ()
		{
		}
	}
}
