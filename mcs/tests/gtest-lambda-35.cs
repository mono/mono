using System;

class C
{
	static int Foo (Func<short> b)
	{
		return 1;
	}

	static int Foo (Func<int> a)
	{
		return 2;
	}

    static int Main()
    {
    	if (Foo (() => 1) != 2)
    		return 1;

    	if (Foo (() => (short) 1) != 1)
    		return 2;

    	if (Foo (() => (byte) 1) != 1)
    		return 3;

        Console.WriteLine ("ok");
        return 0;
    }

}