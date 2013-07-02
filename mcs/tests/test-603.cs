// Compiler options: -unsafe

using System;

namespace ConsoleApplication1
{
	public struct Strct
	{
		public uint a;
		public uint b;
	}
	
	unsafe class Program
	{
		static Strct* ptr = null;
		
		public static int Main ()
		{
			Strct* values = ptr;
			values++;
			values++;
			
			long diff = values - ptr;
			if (diff != 2)
				return 1;
			
			return 0;
		}
	}
}
