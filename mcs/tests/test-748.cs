// Compiler options: -r:test-748-lib.dll

using System;
using Test;
using RealTest;

class M
{
	Foo Test ()
	{
		return new RealTest.Foo ();
	}
	
	public static void Main ()
	{
	}
}

namespace Test.Local
{
	class M
	{
		Foo Test ()
		{
			return new RealTest.Foo ();
		}
	}
}

namespace RealTest
{
	class Foo
	{
	}
}
