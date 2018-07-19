// CS1629: Unsafe code may not appear in iterators
// Line: 17
// Compiler options: -unsafe

using System.Collections.Generic;

public unsafe class TestClass
{
	public struct Foo {
		public bool C;
	}

	Foo *current;

	public IEnumerable<Foo> EnumeratorCurrentEvents ()
	{
		yield return *current;
	}
}