// CS0619: `A' is obsolete: `msg'
// Line: 21

using System;

[Obsolete ("msg", true)]
class A
{
	public class M
	{
		public static void Foo ()
		{
		}
	}
}

class C
{
	public static void Main ()
	{
		A.M.Foo ();
	}
}
