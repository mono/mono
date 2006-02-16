// CS0453: The type `X' must be a non-nullable value type in order to use it as type parameter `T' in the generic type or method `MyValue<T>'
// Line: 10
public class MyValue<T>
	where T : struct
{ }

class X
{
	MyValue<X> x;

	static void Main ()
	{ }
}
