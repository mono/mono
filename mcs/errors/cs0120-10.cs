// CS0120: `TestNamespace.TestClass.HelloWorld()': An object reference is required for the nonstatic field, method or property
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
