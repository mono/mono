// CS0311: The type `object' cannot be used as type parameter `U' in the generic type or method `G<C>.Method<U>()'. There is no implicit reference conversion from `object' to `C'
// Line: 9

public class C
{
	public static void Main ()
	{
		var mc = new G<C> ();
		mc.Method<object> ();
	}
}

public class G<T> where T : C
{
	public void Method<U> () where U : T
	{
	}
}
