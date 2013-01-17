// Inspired by
// http://blogs.msdn.com/ericlippert/archive/2007/03/28/lambda-expressions-vs-anonymous-methods-part-five.aspx

using System;

class TestClass
{	
	delegate void DT (T t);
	delegate void DF (F f);

	struct T { }
	struct F { }
	
	static void P (DT dt)
	{
		Console.WriteLine ("True");
		dt (new T ());
	}

	static void P (DF df)
	{
		System.Console.WriteLine ("False");
		df (new F ());
	}

	static T And (T a1, T a2) { return new T (); }
	static F And (T a1, F a2) { return new F (); }
	static F And (F a1, T a2) { return new F (); }
	static F And (F a1, F a2) { return new F (); }
	
	static T Or (T a1, T a2) { return new T (); }
	static T Or (T a1, F a2) { return new T (); }
	static T Or (F a1, T a2) { return new T (); }
	static F Or (F a1, F a2) { return new F (); }
	
	static F Not (T a) { return new F (); }
	static T Not (F a) { return new T (); }
	
	static void StopTrue (T t) { }

	public static int Main ()
	{
		// Test that we encode (!v3) & ((!v1) & ((v1 | v2) & (v2 | v3)))
		P (v1 => P (v2 => P (v3 => StopTrue (
		  And (Not (v3),
			And (Not (v1),
				And (Or (v1, v2), Or (v2, v3))
				)
			)
		))));
		
		return 0;
	}
}
