// internal dynamic conversion tests

class G<T>
{
}


class C
{
	public static dynamic Create ()
	{
		return 1;
	}
	
	static void M<T> ()
	{
		dynamic d = default (T);
		var v = default (dynamic);
	}
	
	public static int Main ()
	{
		var d = Create ();
		d.ToString ();
		
		M<int> ();
		M<C> ();
		
		G<object> v1 = new G<dynamic>();
		G<dynamic> v2 = new G<object>();
		return 0;
	}
}
