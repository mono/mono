// CS0269: Use of unassigned out parameter `a'
// Line: 23

struct A
{
	public int a;
	public A (int foo)
	{
	    a = foo;
	}
}

class X
{
	static void test_output (A a)
	{
	}
	
	static void test5 (out A a)
	{
		test_output (a);
		a = new A (5);
	}
}
