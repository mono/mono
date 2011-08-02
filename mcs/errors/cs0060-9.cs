// CS0060: Inconsistent accessibility: base class `System.Collections.Generic.List<Foo<T>.Bar>' is less accessible than class `Foo<T>'
// Line: 7

using System;
using System.Collections.Generic;

public class Foo<T> : List<Foo<T>.Bar>
{
	class Bar
	{
	}
}

