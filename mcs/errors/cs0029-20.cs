// CS0029: Cannot implicitly convert type `A<int>.B<long>' to `A<long>.B<long>'
// Line: 14

class A<T>
{
	public class B<U>
	{
	}
}

class Test
{
	static A<int>.B<long> a;
	static A<long>.B<long> b = a;
}
