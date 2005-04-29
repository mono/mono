// Compiler options: -t:library

namespace FLMID.Bugs.Marshal15
{
	public class A<T>

	{

	}
	public abstract class B
	{
		protected A<bool> _aux;
	}
	public class X : B
	{
	}
	public abstract class C
	{
		protected B _layout;
	}
}

