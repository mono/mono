using System;

class OutputParam
{
    public static void Main(string[] args)
    {
	 int a;
	 Method(out a);
	 Console.WriteLine(a);
    }

    public static void Method(out int a)
    {
	int b;

	try {
	    b = 5;
	    return;
	} finally {
	    a = 6;
	}
    }
}
