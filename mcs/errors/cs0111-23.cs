// CS0111: A member `C.Foo<U>(U)' is already defined. Rename this member or use different parameter types
// Line : 12

public class C
{
	void Foo<T> (T i)
	{
	}
	
	void Foo<U> (U i)
	{
	}
}
