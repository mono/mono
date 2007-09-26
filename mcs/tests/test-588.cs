using System;
using TestNamespace;

namespace TestNamespace
{
	public class TestClass
	{
		public static void HelloWorld ()
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
