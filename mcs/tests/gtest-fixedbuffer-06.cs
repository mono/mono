// Compiler options: -unsafe

using System;

namespace Bug
{
	unsafe struct Demo
	{
		fixed bool test [4];
	
		bool Fixed ()
		{
			fixed (bool* data_ptr = test)
			{
				return true;
			}
		}
		
		static bool Foo (int [] data)
		{
			fixed (int* data_ptr = data)
			{
				return data_ptr == null ? true : false;
			}
		}
		
		public static int Main ()
		{
			if (!Foo (null))
				return 1;
			
			if (!Foo (new int [0]))
				return 2;
			
			if (!new Demo().Fixed ())
				return 3;
			
			Console.WriteLine ("OK");
			return 0;
		}
	}
}

