// CS0111: A member `C.Foo<U>(G<U>)' is already defined. Rename this member or use different parameter types
// Line : 14

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
