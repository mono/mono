// cs0122-14.cs: `Test.SomeValue' is inaccessible due to its protection level
// Line: 7
// Compiler options: -r:CS0122-14-lib.dll

public class MyEnum
{
	int Unknown = Test.SomeValue;
	static void Main () {}
}

