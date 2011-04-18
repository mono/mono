// CS0122: `Test.foo' is inaccessible due to its protection level
// Line: 10

internal class Test 
{
	protected const int foo = 0;
}
internal class Rest
{
	protected const int foo = Test.foo;

	static void Main () {}
}
