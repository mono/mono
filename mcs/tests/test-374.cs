internal class Test 
{
	protected internal const int foo = 0;
}
internal class Rest
{
	protected const int foo = Test.foo;

	public static void Main () {}
}
