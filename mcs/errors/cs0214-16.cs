// CS0214: Pointers and fixed size buffers may only be used in an unsafe context
// Line: 8 

public class G<T> {}

abstract class A
{
	public abstract G<int*[]> Foo1 ();
}
