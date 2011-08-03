// Compiler options: -doc:xml-057.xml /warnaserror /warn:4

namespace Test
{
	using System;

	/// <summary>Documentation Text</summary>
	public delegate void FirstTestDelegate<T> (T obj) where T : Exception;

	/// <summary>test</summary>
	public interface TestInterface { }
}

class A
{
	static void Main ()
	{
	}
}

