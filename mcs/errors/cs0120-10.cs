// CS0120: An object reference is required to access non-static member `TestNamespace.TestClass.HelloWorld()'
// Line: 31

using System;
using TestNamespace;

namespace TestNamespace
{
	public class TestClass
	{
		public void HelloWorld ()
		{
		}
	}
}

class SuperClass
{
	TestClass tc = null;

	TestClass TestClass
	{
		get { return tc; }
	}
}

class SubClass : SuperClass
{
	public SubClass ()
	{
		TestClass.HelloWorld ();
	}
}

class App
{
	public static void Main ()
	{
		SubClass sc = new SubClass ();
	}
}
