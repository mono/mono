// Compiler options: -langversion:default
// Anonymous method fix, implicit conversion inside an old-style constructor
// Bug 70150
using System;
public delegate double Mapper (int item);

class X
{
        public static int Main ()
        {
                Mapper mapper = new Mapper (delegate (int i){
			return i * 12; });

		if (mapper (3) == 36)
			return 0;

		// Failure
		return 1;
        }
}
