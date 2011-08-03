// CS0101: The namespace `global::' already contains a definition for `SomeEnum'
// Line: 10
using System;

public enum SomeEnum {
	Something1,
	Something2
}

public enum SomeEnum {
	Dog,
	Fish,
	Cat
}

public class DupeEnumTest {
	public static void Main(string[] args) {
		SomeEnum someEnum = SomeEnum.Dog;
		Console.WriteLine("SomeEnum Result: " + someEnum.ToString
());
	}
}





        
