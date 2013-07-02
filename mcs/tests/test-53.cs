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

class X {
	public static int Main ()
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
		
		MyDispose bb = new MyDispose ();
		using (bb){
			
		}
		if (bb.disposed == false)
			return 6;
		
		Console.WriteLine ("All tests pass");
		return 0;
	}
}
	
