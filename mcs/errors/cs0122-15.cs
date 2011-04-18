// CS0122: `Test.SomeValue' is inaccessible due to its protection level
// Line: 7
// Compiler options: -r:CS0122-15-lib.dll

public class MyEnum
{
	int Unknown = Test.SomeValue;
	static void Main () {}
}

