// Compiler options: -unsafe

using System;

namespace MonoPointerBugTest
{
	struct MyStructure
	{
		int q;
		int z;
		int a;
	}

	class Program
	{
		public static int Main ()
		{
			unsafe {
				MyStructure structure = new MyStructure ();

				MyStructure* pointer1 = &structure;
				MyStructure* pointer2 = pointer1;

				//on the Mac this works like: pointer2++;
				pointer2 += 10;

				int difference = (int) ((byte*) pointer2 - (byte*) pointer1);
				if (difference != 120)
					return 1;

				return 0;
			}
		}
	}
}
