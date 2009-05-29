// CS0111: A member `C.Foo<U>(U)' is already defined. Rename this member or use different parameter types
// Line : 12

class G<T>
{
}

public class C
{
	void Foo<T> (G<T> g)
	{
	}
	
	void Foo<U> (G<U> u)
	{
	}
}
