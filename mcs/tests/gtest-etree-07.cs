// Compiler options: -unsafe

using System;
using System.Linq.Expressions;

delegate void EmptyDelegate ();
unsafe delegate int* UnsafeDelegate ();

class C
{
	static int i;
	
	static void Test ()
	{
		i += 9;
	}
	
	static unsafe int* Foo ()
	{
		return (int*)1;
	}
	
	void M ()
	{
	}
	
	int TestInstance ()
	{
		Expression<Func<EmptyDelegate>> e = () => M;
		Console.WriteLine (e.Body.ToString ());
		if (e.Body.ToString () != "Convert(CreateDelegate(EmptyDelegate, value(C), Void M()))")
			return 1;
		
		e.Compile () ();
		return 0;
	}
	
	public static int Main ()
	{
		Expression<Func<EmptyDelegate>> e = () => new EmptyDelegate (Test);
		
		if (e.Body.ToString () != "Convert(CreateDelegate(EmptyDelegate, null, Void Test()))")
			return 1;

		var v = e.Compile ();
		v.Invoke ()();
		
		if (i != 9)
			return 2;
		
		Expression<Func<EmptyDelegate>> e2 = () => Test;
		if (e2.Body.ToString () != "Convert(CreateDelegate(EmptyDelegate, null, Void Test()))")
			return 3;

		var v2 = e2.Compile ();
		v2.Invoke ()();
		
		if (i != 18)
			return 4;
			
		unsafe {
			Expression<Func<UnsafeDelegate>> e3 = () => new UnsafeDelegate (Foo);
			if (e3.Body.ToString () != "Convert(CreateDelegate(UnsafeDelegate, null, Int32* Foo()))")
				return 5;
			
			var v3 = e3.Compile ();
			if (v3.Invoke ()() != (int*)1)
				return 6;
		}
		
		if (new C ().TestInstance () != 0)
			return 7;

		Console.WriteLine ("OK");
		return 0;
	}
}

