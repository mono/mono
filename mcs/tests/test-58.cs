// Compiler options: -unsafe

using System;
using System.Reflection;

public class Blah {
	
	public static int Main ()
	{
		unsafe {
			int* i;
			int foo = 10;

			void* bar;

			i = &foo;

			bar = i;
			
			Console.WriteLine ("Address : {0}", (int) i);
		}

		return 0;
	}	
}

		
