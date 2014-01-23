// Compiler options: -unsafe

using System;

namespace ConsoleApplication6
{
	unsafe class Program
	{
		static int a;
		static int* the_ptr = (int*) 0xdeadbeaf;
		static int** the_pptr = (int**) 0xdeadbeaf;

		public static void Main ()
		{
			Console.WriteLine ("TEST: {0:x}", new IntPtr (the_pptr).ToInt64 ());

			fixed (int* a_ptr = &a) {
				Console.WriteLine (new IntPtr (a_ptr));

				int*[] array = { the_ptr };
				int*[] array2 = { a_ptr };
				int* ptr = the_ptr;
				int** pptr = the_pptr;

				fixed (int** pptr2 = &the_ptr) {
					Console.WriteLine (new IntPtr (pptr));
				}
			}
		}
	}
}
