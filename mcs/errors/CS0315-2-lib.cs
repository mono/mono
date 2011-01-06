public class A<T> where T : A<T>.N1<T>
{
	public class N1<U>
	{
	}
	
	public void Foo (N1<int> arg)
	{
	}
}
