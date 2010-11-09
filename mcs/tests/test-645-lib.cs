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

public class T3 : T2.E
{
}

public class T1
{
	public void Test ()
	{
	}
}

public class T2 : T1
{
	public interface E
	{
	}
}
