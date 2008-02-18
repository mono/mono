// CS1922: A field or property `Data.Value' cannot be initialized with a collection object initializer because type `int' does not implement `System.Collections.IEnumerable' interface
// Line: 16


using System;

class Data
{
	public int Value;
}

public class Test
{
	static void Main ()
	{
		var c = new Data { Value = { 0, 1, 2 } };
	}
}
