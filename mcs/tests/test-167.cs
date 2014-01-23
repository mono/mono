//
// See bug 31834 for details about this bug
//

using System;

class X
{
        static int Test (params Foo[] foo)
        { 
		if (foo.Length != 1)
			return 1;

		if (foo [0] != Foo.A)
			return 2;

		return 0;	
	}

        enum Foo {
                A, B
        }

        public static int Main ()
        {
                int v = Test (Foo.A);
		if (v != 0)
			return v;

		MyEnum [] arr = new MyEnum [2];
		arr [0] = MyEnum.c;

		if (arr [0] != MyEnum.c)
			return 3;
		return 0;
        }

        enum MyEnum {a,b,c};
}
