//
// Tests the using statement implementation
//
using System;
using System.IO;

class MyDispose : IDisposable {
	public bool disposed;
	
	public void Dispose ()
	{
		disposed = true;
	}
}

//
// This class does not implement IDiposable, but has an implicit conversion
// defined
//
class NoIDispose {
	static public MyDispose x;

	public NoIDispose ()
	{
	}
	
	static NoIDispose ()
	{
		x = new MyDispose ();
	}
	
	public static implicit operator MyDispose (NoIDispose a)
	{
		return x;
	}
}

class Y {
	static void B ()
	{
		using (NoIDispose a = new NoIDispose ()){
		}
	}
	
}

class X {
	static int Main ()
	{
		MyDispose copy_a, copy_b, copy_c;

		//
		// Test whether the two `a' and `b' get disposed
		//
		using (MyDispose a = new MyDispose (), b = new MyDispose ()){
			copy_a = a;
			copy_b = b;
		}

		if (!copy_a.disposed)
			return 1;
		if (!copy_b.disposed)
			return 2;

		Console.WriteLine ("Nested using clause disposed");

		//
		// See if the variable `b' is disposed if there is
		// an error thrown inside the using block.
		//
		copy_c = null;
		try {
			using (MyDispose c = new MyDispose ()){
				copy_c = c;
				throw new Exception ();
			}
		} catch {}

		if (!copy_c.disposed)
			return 3;
		else
			Console.WriteLine ("Disposal on finally block works");

		//
		// This should test if `a' is non-null before calling dispose
		// implicitly
		//
		using (MyDispose d = null){
		}

		Console.WriteLine ("Null test passed");
		
		//
		// This tests that a variable is permitted here if there is
		// an implicit conversion to a type that implement IDisposable
		//
		using (NoIDispose a = new NoIDispose ()){
		}

		//
		// See if we dispose the object that can be implicitly converted
		// to IDisposable 
		if (NoIDispose.x.disposed != true)
			return 4;
		else
			Console.WriteLine ("Implicit conversion from type to IDisposable pass");

		MyDispose bb = new MyDispose ();
		using (bb){
			
		}
		if (bb.disposed == false)
			return 6;
		
		Console.WriteLine ("All tests pass");
		return 0;
	}
}
	
