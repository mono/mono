// Compiler options: -unsafe

//
// Checks for an implicit void * conversion during an
// explicit conversion
//

using System;

namespace IntPtr_Conv
{
	struct FooStruct {
		int x;
	}

	class Class1 {
		
		public static int Main(string[] args)
		{
			IntPtr p = IntPtr.Zero;

			unsafe {
				FooStruct* s = (FooStruct*) (p);
			}

			return 0;
		}
	}
}
