using System;

namespace IntPtr_Conv
{
	struct FooStruct {
		int x;
	}

	class Class1 {
		
		static int Main(string[] args)
		{
			IntPtr[] pArray = new IntPtr[1] {IntPtr.Zero};

			unsafe {
				FooStruct* s = (FooStruct*) (pArray[0]);
			}

			return 0;
		}
	}
}

