// CS0200: Property or indexer `MyClass.Type' cannot be assigned to (it is read-only)
// Line: 12

using System;

 public class MyClass
 {
	Type Type { get; }

	public void Test ()
	{
		Type = typeof (string);
	}
}