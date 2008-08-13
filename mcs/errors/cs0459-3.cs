// CS0459: Cannot take the address of using variable `m'
// Line: 19
// Compiler options: -unsafe

using System;

struct S : IDisposable
{
	public void Dispose ()
	{
	}
}

class X {

	unsafe static void Main ()
	{
		using (S m = new S ()){
			S* mm = &m;
		}
	}
}
	
