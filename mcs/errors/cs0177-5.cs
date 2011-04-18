// CS0177: The out parameter `a' must be assigned to before control leaves the current method
// Line: 21

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
	} catch (Exception) { return; }

	a = b;
    }
}
